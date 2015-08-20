using System;
using UnityEngine;
using UnityEngine.Rendering;


public class WaterBehaviorNew : Singleton<WaterBehaviorNew>
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


	public const int MaxDrops = 1024;
	public const int WorkSize = 32;
	public const int NWorkGroups = MaxDrops / WorkSize;

	public ComputeShader WaterUpdateShader;
	public Material WaterDropsRenderMat;

	public int NBursts { get; private set; }
	public ComputeBuffer DropsBuffer { get; private set; }

	private int burstKernel, updateKernel;
	private Texture2D randTex;


	protected override void Awake()
	{
		base.Awake();

		burstKernel = WaterUpdateShader.FindKernel("Burst");
		updateKernel = WaterUpdateShader.FindKernel("Update");

		WaterDropNew[] data = new WaterDropNew[MaxDrops];
		for (int i = 0; i < MaxDrops; ++i)
		{
			data[i] = new WaterDropNew(Vector2.zero, Vector2.zero, 0.0f);
		}

		DropsBuffer = new ComputeBuffer(data.Length, sizeof(float) * 5);
		DropsBuffer.SetData(data);

		NBursts = 0;


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
		WaterUpdateShader.SetTexture(burstKernel, "randVals", randTex);
		WaterUpdateShader.SetBuffer(burstKernel, "drops", DropsBuffer);
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
		WaterUpdateShader.Dispatch(updateKernel, NBursts, 1, 1);
	}
	void OnRenderObject()
	{
		//Don't render if the camera doesn't see this object's layer.
		if ((Camera.current.cullingMask & (1 << gameObject.layer)) == 0)
		{
			return;
		}

		WaterDropsRenderMat.SetPass(0);
		WaterDropsRenderMat.SetBuffer("dropsBuffer", DropsBuffer);
		Graphics.DrawProcedural(MeshTopology.Points, WorkSize * NBursts);
	}
	void OnDestroy()
	{
		if (DropsBuffer != null)
		{
			DropsBuffer.Dispose();
			DropsBuffer = null;
		}
	}


	/// <summary>
	/// Bursts a set number of water drops with varied parameters within the given ranges.
	/// </summary>
	public void BurstDrops(Vector2 minPos, Vector2 maxPos,
						   Vector2 minVel, Vector2 maxVel,
						   float minRadius, float maxRadius)
	{
		WaterUpdateShader.SetFloats("minPos", minPos.x, minPos.y);
		WaterUpdateShader.SetFloats("maxPos", maxPos.x, maxPos.y);
		WaterUpdateShader.SetFloats("minVel", minVel.x, minVel.y);
		WaterUpdateShader.SetFloats("maxVel", maxVel.x, maxVel.y);
		WaterUpdateShader.SetFloat("minRadius", minRadius);
		WaterUpdateShader.SetFloat("maxRadius", maxRadius);

		WaterUpdateShader.SetInt("burstNumber", NBursts);

		WaterUpdateShader.SetInt("indexOffset", (NBursts % NWorkGroups) * WorkSize);
		NBursts += 1;

		WaterUpdateShader.Dispatch(burstKernel, 1, 1, 1);
	}
}