using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// Handles bursting, updating, and rendering GPU water drops.
/// Everything is done on the GPU with the help of Compute Shaders.
/// This controller manually keeps track of when each burst of water needs to be destroyed.
/// </summary>
public class WaterController : Singleton<WaterController>
{
	//IMPORTANT NOTE: if changing this, also change the work size in WaterUpdate, WaterSwap, and WaterBurst.
	public const int WorkSize = 64;

	private const int Stride_Drops = sizeof(float) * 5,
					  Stride_AreDropsDead = sizeof(float) * 3;


	public int MaxDrops { get { return WaterConstants.Instance.MaxDrops; } }
	public int MaxBursts { get { return MaxDrops / WorkSize; } }
	public int NBursts { get; private set; }

	public ComputeBuffer DropsBuffer { get; private set; }


	public ComputeShader WaterBurstShader, WaterUpdateShader, WaterSwapShader;
	public Material WaterDropsRenderMat;

	private int burstKernel, updateKernel, swapKernel;
	private Texture2D randTex;

	private struct Burst
	{
		public float TimeLeft;
		public bool Exists;
		public Burst(float timeLeft, bool exists) { TimeLeft = timeLeft; Exists = exists; }
	}
	private List<Burst> bursts;

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct WaterDrop
	{
		public Vector2 Pos, Vel;
		public float Radius;
	}
	private WaterDrop[] dropsBufferCPU = new WaterDrop[WorkSize];


	protected void Start()
	{
		burstKernel = WaterBurstShader.FindKernel("Burst");
		updateKernel = WaterUpdateShader.FindKernel("Update");
		swapKernel = WaterSwapShader.FindKernel("Swap");

		//"Drops" buffer.
		byte[] dropData = new byte[MaxDrops * Stride_Drops];
		DropsBuffer = new ComputeBuffer(MaxDrops, Stride_Drops);
		DropsBuffer.SetData(dropData);

		//"Bursts" list.
		int maxBursts = MaxDrops / WorkSize;
		bursts = new List<Burst>(maxBursts);
		for (int i = 0; i < maxBursts; ++i)
			bursts.Add(new Burst(0.0f, false));


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


		//Set buffer/texture data for compute shaders.
		WaterBurstShader.SetTexture(burstKernel, "randVals", randTex);
		WaterBurstShader.SetBuffer(burstKernel, "drops", DropsBuffer);
		WaterUpdateShader.SetBuffer(updateKernel, "drops", DropsBuffer);
		WaterSwapShader.SetBuffer(swapKernel, "toSwap", DropsBuffer);
	}
	void FixedUpdate()
	{
		if (WorldTime.IsPaused || WorldVoxels.Instance.VoxelTex == null)
		{
			return;
		}


		//Update the drops.

		WaterUpdateShader.SetFloat("deltaTime", WorldTime.FixedDeltaTime);

		//TODO: Once these constants are set in stone, don't waste time updating them every frame.
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
		

		//Update the CPU-side info.
		float radiusChange = -WaterConstants.Instance.RadiusShrinkRate * WorldTime.FixedDeltaTime;
		for (int i = 0; i < NBursts; ++i)
		{
			Assert.IsTrue(bursts[i].Exists);
			bursts[i] = new Burst(bursts[i].TimeLeft - WorldTime.FixedDeltaTime, true);

			if (bursts[i].TimeLeft <= 0.0f)
			{
				//Get the water drops that need to be killed, and kill them on the GPU and CPU sides.
				bursts[i] = new Burst(0.0f, false);
				if (i > 0)
				{
					SwapBursts(0, i);
				}
				DropsBuffer.GetData(dropsBufferCPU);
				if (NBursts > 1)
				{
					SwapBursts(0, NBursts - 1);
				}

				NBursts -= 1;

				//Kill them.
				for (int j = 0; j < WorkSize; ++j)
					KillDrop(dropsBufferCPU[j].Pos);
			}
		}
	}
	void OnRenderObject()
	{
		//Don't render if the camera doesn't see this object's layer.
		if (NBursts == 0 || (Camera.current.cullingMask & (1 << gameObject.layer)) == 0)
		{
			return;
		}

		WaterDropsRenderMat.SetPass(0);
		WaterDropsRenderMat.SetBuffer("dropsBuffer", DropsBuffer);
		Graphics.DrawProcedural(MeshTopology.Points, NBursts * WorkSize);
	}
	void OnDestroy()
	{
		DisposeBuffer(DropsBuffer);
		DropsBuffer = null;

		randTex = null;
	}
	private void DisposeBuffer(ComputeBuffer buff)
	{
		if (buff != null)
		{
			buff.Dispose();
			buff = null;
		}
	}

	private void SwapBursts(int first, int second)
	{
		WaterSwapShader.SetInt("firstIndex", first * WorkSize);
		WaterSwapShader.SetInt("secondIndex", second * WorkSize);
		WaterSwapShader.Dispatch(swapKernel, 1, 1, 1);

		Burst temp = bursts[first];
		bursts[first] = bursts[second];
		bursts[second] = temp;
	}

	/// <summary>
	/// Processes a drop at the given position that just evaporated.
	/// Contributes some wetness to whatever surface(s) it was touching.
	/// </summary>
	private void KillDrop(Vector2 pos)
	{
		VoxelTypes[,] vxs = WorldVoxels.Instance.Voxels;
		float[,] wets = WorldVoxels.Instance.Wetness;

		Vector2i posI = new Vector2i((int)pos.x, (int)pos.y);

		if (posI.x < 0 || posI.y < 0 || posI.x >= vxs.GetLength(0) || posI.y >= vxs.GetLength(1))
		{
			Debug.Log("Killing drop that is outside world");
			return;
		}


		//It might be touching up to two surfaces: floor/ceiling and a wall.

		bool[] horzAndVert = new bool[2] { false, false };
		bool[] isLesserSurface = new bool[2];

		float horzDist = Mathf.Abs(pos.x - (float)posI.x);
		if (horzDist < WaterConstants.Instance.DropSurfaceThreshold)
		{
			horzAndVert[0] = true;
			isLesserSurface[0] = true;
		}
		else if (horzDist > (1.0f - WaterConstants.Instance.DropSurfaceThreshold))
		{
			horzAndVert[0] = true;
			isLesserSurface[0] = false;
		}

		float vertDist = Mathf.Abs(pos.y - (float)posI.y);
		if (vertDist < WaterConstants.Instance.DropSurfaceThreshold)
		{
			horzAndVert[1] = true;
			isLesserSurface[1] = true;
		}
		else if (vertDist > (1.0f - WaterConstants.Instance.DropSurfaceThreshold))
		{
			horzAndVert[1] = true;
			isLesserSurface[1] = false;
		}


		//Add wetness to one or both surfaces touching the drop.
		Vector2i horzPos = (isLesserSurface[0] ? posI.LessX : posI.MoreX),
				 vertPos = (isLesserSurface[1] ? posI.LessY : posI.MoreY);
		if (horzAndVert[0] && horzPos.x >= 0 && horzPos.x < vxs.GetLength(0) &&
			WorldVoxels.IsSolid(WorldVoxels.GetVoxelAt(horzPos)))
		{
			if (horzAndVert[1] && vertPos.y >= 0 && vertPos.y < vxs.GetLength(1) &&
				WorldVoxels.IsSolid(WorldVoxels.GetVoxelAt(vertPos)))
			{
				float wetness = WaterConstants.Instance.DropWetness * 0.5f;
				wets[horzPos.x, horzPos.y] = Mathf.Min(1.0f, wets[horzPos.x, horzPos.y] + wetness);
				wets[vertPos.x, vertPos.y] = Mathf.Min(1.0f, wets[vertPos.x, vertPos.y] + wetness);
			}
			else
			{
				wets[horzPos.x, horzPos.y] = Mathf.Min(wets[horzPos.x, horzPos.y] +
														WaterConstants.Instance.DropWetness,
													   1.0f);
			}
		}
		else if (horzAndVert[1] && vertPos.y >= 0 && vertPos.y < vxs.GetLength(1) &&
				 WorldVoxels.IsSolid(WorldVoxels.GetVoxelAt(vertPos)))
		{
			wets[vertPos.x, vertPos.y] = Mathf.Min(wets[vertPos.x, vertPos.y] +
													WaterConstants.Instance.DropWetness,
												   1.0f);
		}
	}

	/// <summary>
	/// Bursts a set number of water drops with varied parameters within the given ranges.
	/// </summary>
	public void BurstDrops(Vector2 minPos, Vector2 maxPos,
						   Vector2 minVel, Vector2 maxVel)
	{
		if ((NBursts * WorkSize) >= MaxDrops)
		{
			return;
		}

		WaterBurstShader.SetFloats("minPos", minPos.x, minPos.y);
		WaterBurstShader.SetFloats("maxPos", maxPos.x, maxPos.y);
		WaterBurstShader.SetFloats("minVel", minVel.x, minVel.y);
		WaterBurstShader.SetFloats("maxVel", maxVel.x, maxVel.y);

		//TODO: Once this constant is set in stone, don't waste time updating it every frame.
		WaterBurstShader.SetFloat("dropRadius", WaterConstants.Instance.DropRadius);

		WaterBurstShader.SetInt("firstBurstIndex", NBursts);

		WaterBurstShader.SetInt("randTexY", NBursts % randTex.height);
		WaterBurstShader.SetInt("randTexWidth", randTex.width);

		WaterBurstShader.SetBuffer(burstKernel, "drops", DropsBuffer);

		WaterBurstShader.Dispatch(burstKernel, 1, 1, 1);

		Assert.IsFalse(bursts[NBursts].Exists);
		bursts[NBursts] = new Burst(WaterConstants.Instance.DropRadius /
										WaterConstants.Instance.RadiusShrinkRate,
									true);
		NBursts += 1;
	}
}