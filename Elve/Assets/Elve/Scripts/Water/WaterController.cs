using System;
using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
/// Handles bursting, updating, and rendering GPU water drops.
/// </summary>
public class WaterController : Singleton<WaterController>
{
	private struct WaterDropNew
	{
		public Vector2 pos, vel;
		public float radius;

		public WaterDropNew(Vector2 _pos, Vector2 _vel, float _radius)
		{
			pos = _pos;
			vel = _vel;
			radius = _radius; 
		}
	}


	public const int WorkSize = 64;
	public int MaxDrops { get { return WaterConstants.Instance.MaxDrops; } }

	public int NDrops { get; private set; }

	public ComputeShader WaterBurstShader, WaterUpdateShader;
	public Material WaterDropsRenderMat;

	public int NBursts { get; private set; }
	public ComputeBuffer DropsBuffer { get; private set; }

	private int burstKernel, updateKernel;
	private Texture2D randTex;


	protected void Start()
	{
		burstKernel = WaterBurstShader.FindKernel("Burst");
		updateKernel = WaterUpdateShader.FindKernel("Update");

		WaterDropNew[] data = new WaterDropNew[MaxDrops];
		for (int i = 0; i < MaxDrops; ++i)
		{
			data[i] = new WaterDropNew(Vector2.zero, Vector2.zero, 0.0f);
		}

		DropsBuffer = new ComputeBuffer(data.Length, sizeof(float) * 5, ComputeBufferType.Append);
		DropsBuffer.SetData(data);

		NBursts = 0;
		NDrops = 0;


		//Set up the rand texture.

		randTex = new Texture2D(MaxDrops, 32, TextureFormat.RGBAHalf, false, true);
		randTex.name = "Water Rand Values";
		randTex.anisoLevel = 0;
		randTex.filterMode = FilterMode.Point;
		randTex.wrapMode = TextureWrapMode.Repeat;

		const float seed1 = 12.5463f,
					seed2 = -2.13551f,
					seed3 = 0.15612f,
					seed4 = 235.12412f,
				    multiple = 23.0f;
		Color[] vals = new Color[randTex.width * randTex.height * 4];
		for (int i = 0; i < vals.Length; ++i)
		{
			float xSeed = (float)i * multiple;
			vals[i].r = NoiseAlgos2D.WhiteNoise(new Vector2(xSeed, seed1));
			vals[i].g = NoiseAlgos2D.WhiteNoise(new Vector2(xSeed, seed2));
			vals[i].b = NoiseAlgos2D.WhiteNoise(new Vector2(xSeed, seed3));
			vals[i].a = NoiseAlgos2D.WhiteNoise(new Vector2(xSeed, seed4));
		}
		randTex.SetPixels(vals);
		randTex.Apply();
		

		//Set buffer/texture data for compute shader.
		WaterBurstShader.SetTexture(burstKernel, "randVals", randTex);
		WaterBurstShader.SetBuffer(burstKernel, "drops", DropsBuffer);
		WaterUpdateShader.SetBuffer(updateKernel, "drops", DropsBuffer);
	}
	void FixedUpdate()
	{
		if (WorldTime.IsPaused || WorldVoxels.Instance.VoxelTex == null)
		{
			return;
		}

		//TODO: Once these constants are set in stone, don't waste time updating them every frame.
		WaterUpdateShader.SetFloat("deltaTime", WorldTime.FixedDeltaTime);
		WaterUpdateShader.SetFloat("radiusShrinkRate", WaterConstants.Instance.RadiusShrinkRate);
		WaterUpdateShader.SetFloat("gravity", WaterConstants.Instance.Gravity);
		WaterUpdateShader.SetFloat("bounceDamp", WaterConstants.Instance.BounceDamp);
		WaterUpdateShader.SetFloat("maxSpeed", WaterConstants.Instance.MaxSpeed);
		WaterUpdateShader.SetFloat("separationForce", WaterConstants.Instance.SeparationForce);
		WaterUpdateShader.SetFloat("normalForce", WaterConstants.Instance.NormalForce);
		WaterUpdateShader.SetFloat("normalForceGrowth", WaterConstants.Instance.NormalForceGrowth);
		
		WaterUpdateShader.SetTexture(updateKernel, "voxelGrid", WorldVoxels.Instance.VoxelTex);
		WaterUpdateShader.SetBuffer(updateKernel, "drops", DropsBuffer);
		WaterUpdateShader.Dispatch(updateKernel, NBursts, 1, 1);
	}
	void OnRenderObject()
	{
		//Don't render if the camera doesn't see this object's layer.
		if (NDrops == 0 || (Camera.current.cullingMask & (1 << gameObject.layer)) == 0)
		{
			return;
		}

		WaterDropsRenderMat.SetPass(0);
		WaterDropsRenderMat.SetBuffer("dropsBuffer", DropsBuffer);
		Graphics.DrawProcedural(MeshTopology.Points, NDrops);
	}
	void OnDestroy()
	{
		if (DropsBuffer != null)
		{
			DropsBuffer.Dispose();
			DropsBuffer = null;
		}
		randTex = null;
	}


	/// <summary>
	/// Bursts a set number of water drops with varied parameters within the given ranges.
	/// </summary>
	public void BurstDrops(Vector2 minPos, Vector2 maxPos,
						   Vector2 minVel, Vector2 maxVel,
						   float minRadius, float maxRadius,
						   int nDrops)
	{
		//Limit the drops to not exceed the maximum.
		nDrops = Mathf.Min(MaxDrops - NDrops, nDrops);
		if (nDrops <= 0)
		{
			return;
		}

		WaterBurstShader.SetFloats("minPos", minPos.x, minPos.y);
		WaterBurstShader.SetFloats("maxPos", maxPos.x, maxPos.y);
		WaterBurstShader.SetFloats("minVel", minVel.x, minVel.y);
		WaterBurstShader.SetFloats("maxVel", maxVel.x, maxVel.y);
		WaterBurstShader.SetFloat("minRadius", minRadius);
		WaterBurstShader.SetFloat("maxRadius", maxRadius);

		WaterBurstShader.SetInt("amountToBurst", nDrops);
		WaterBurstShader.SetInt("randTexY", NBursts % randTex.height);
		WaterBurstShader.SetInt("randTexWidth", randTex.width);

		WaterBurstShader.SetBuffer(burstKernel, "drops", DropsBuffer);

		WaterBurstShader.Dispatch(burstKernel, (nDrops / WorkSize) + 1, 1, 1);
		NBursts += 1;
		NDrops += nDrops;
	}
}