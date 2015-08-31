using System;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;


/// <summary>
/// Handles bursting, updating, and rendering GPU water drops.
/// </summary>
public class WaterController : Singleton<WaterController>
{
	public const int WorkSizeUpdate = 64,
					 WorkSizeDestroy = 64;
	private const int Stride_Drops = sizeof(float) * 5,
					  Stride_AreDropsDead = sizeof(float) * 3;


	public int MaxDrops { get { return WaterConstants.Instance.MaxDrops; } }

	public int NDrops { get; private set; }
	public int NBursts { get; private set; }

	public ComputeBuffer DropsBuffer { get; private set; }


	public ComputeShader WaterBurstShader, WaterUpdateShader, WaterDestroyShader;
	public Material WaterDropsRenderMat;

	private ComputeBuffer areDropsDeadBuffer,
						  deadDropsBuffer,
						  countBuffer;
	DeadWaterDrop[] deadDropsData;

	private int burstKernel, updateKernel, initDestroyKernel, findDestroyKernel, destroyKernel;
	private Texture2D randTex;

	private int nDeadDrops = 0;


	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct DeadWaterDrop
	{
		public Vector2 pos;
		public float radius;
	}


	protected void Start()
	{
		burstKernel = WaterBurstShader.FindKernel("Burst");
		updateKernel = WaterUpdateShader.FindKernel("Update");
		initDestroyKernel = WaterDestroyShader.FindKernel("InitBuffer");
		findDestroyKernel = WaterDestroyShader.FindKernel("FindDeadDrops");
		destroyKernel = WaterDestroyShader.FindKernel("DestroyDrops");

		//Main "water drop" buffer.
		byte[] dropData = new byte[MaxDrops * Stride_Drops];
		DropsBuffer = new ComputeBuffer(MaxDrops, Stride_Drops, ComputeBufferType.Append);
		DropsBuffer.SetData(dropData);

		//List of which drops need to be killed.
		byte[] testDeathData = new byte[MaxDrops * Stride_AreDropsDead];
		areDropsDeadBuffer = new ComputeBuffer(MaxDrops, Stride_AreDropsDead);
		areDropsDeadBuffer.SetData(testDeathData);

		//Buffer of drops that have been killed.
		deadDropsData = new DeadWaterDrop[MaxDrops];
		for (int i = 0; i < MaxDrops; ++i)
		{
			deadDropsData[i] = new DeadWaterDrop();
		}
		deadDropsBuffer = new ComputeBuffer(deadDropsData.Length,
											Marshal.SizeOf(typeof(DeadWaterDrop)),
											ComputeBufferType.Append);
		deadDropsBuffer.SetData(deadDropsData);

		//Buffer that just holds the size of another buffer.
		countBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.DrawIndirect);
		int[] countDat = new int[4] { 0, 0, 0, 0 };
		countBuffer.SetData(countDat);

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
		

		//Set buffer/texture data for compute shaders.
		WaterBurstShader.SetTexture(burstKernel, "randVals", randTex);
		WaterBurstShader.SetBuffer(burstKernel, "drops", DropsBuffer);
		WaterUpdateShader.SetBuffer(updateKernel, "drops", DropsBuffer);
		WaterDestroyShader.SetBuffer(initDestroyKernel, "toClear", deadDropsBuffer);
		WaterDestroyShader.SetBuffer(findDestroyKernel, "dropsCopy", areDropsDeadBuffer);
		WaterDestroyShader.SetBuffer(findDestroyKernel, "dropsToCheck", DropsBuffer);
		WaterDestroyShader.SetBuffer(destroyKernel, "dropsCopy", areDropsDeadBuffer);
		WaterDestroyShader.SetBuffer(destroyKernel, "drops", DropsBuffer);
		WaterDestroyShader.SetBuffer(destroyKernel, "deadDrops", deadDropsBuffer);
	}
	void FixedUpdate()
	{
		if (WorldTime.IsPaused || WorldVoxels.Instance.VoxelTex == null)
		{
			return;
		}


		//Update the drops.

		int nWorkGroupsUpdate = (NDrops / WorkSizeUpdate) + 1;
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
		WaterUpdateShader.Dispatch(updateKernel, nWorkGroupsUpdate, 1, 1);


		//Now find and kill any drops that need to be killed off.

		int nWorkGroupsDestroy;
		if (nDeadDrops > 0)
		{
			nWorkGroupsDestroy = (nDeadDrops / WorkSizeDestroy) + 1;
			WaterDestroyShader.SetInt("nTotalDrops", nDeadDrops);
			WaterDestroyShader.SetBuffer(initDestroyKernel, "toClear", deadDropsBuffer);
			WaterDestroyShader.Dispatch(initDestroyKernel, nWorkGroupsDestroy, 1, 1);
		}

		nWorkGroupsDestroy = (NDrops / WorkSizeDestroy) + 1;
		WaterDestroyShader.SetInt("nTotalDrops", NDrops);
		WaterDestroyShader.SetBuffer(findDestroyKernel, "dropsCopy", areDropsDeadBuffer);
		WaterDestroyShader.SetBuffer(findDestroyKernel, "dropsToCheck", DropsBuffer);
		WaterDestroyShader.Dispatch(findDestroyKernel, nWorkGroupsDestroy, 1, 1);
		WaterDestroyShader.SetBuffer(destroyKernel, "dropsCopy", areDropsDeadBuffer);
		WaterDestroyShader.SetBuffer(destroyKernel, "drops", DropsBuffer);
		WaterDestroyShader.SetBuffer(destroyKernel, "deadDrops", deadDropsBuffer);
		WaterDestroyShader.Dispatch(destroyKernel, nWorkGroupsDestroy, 1, 1);


		//Pull onto the CPU the destroyed drops and calculate their effect on the world.
		nDeadDrops = GetBufferSize(deadDropsBuffer);
		if (nDeadDrops > 0)
		{
			deadDropsBuffer.GetData(deadDropsData);
			for (int i = 0; i < nDeadDrops; ++i)
			{
				KillDrop(deadDropsData[i]);
			}
			NDrops -= nDeadDrops;
			UnityEngine.Assertions.Assert.IsTrue(NDrops >= 0);
		}
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
		DisposeBuffer(DropsBuffer);
		DropsBuffer = null;
		
		DisposeBuffer(areDropsDeadBuffer);
		areDropsDeadBuffer = null;
		
		DisposeBuffer(deadDropsBuffer);
		deadDropsBuffer = null;
		
		DisposeBuffer(countBuffer);
		countBuffer = null;
	
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

	/// <summary>
	/// Processes a drop at the given position that just disappeared.
	/// Contributes some wetness to whatever surface it was touching.
	/// </summary>
	private void KillDrop(DeadWaterDrop drop)
	{
		VoxelTypes[,] vxs = WorldVoxels.Instance.Voxels;
		float[,] wets = WorldVoxels.Instance.Wetness;

		Vector2i posI = new Vector2i((int)drop.pos.x, (int)drop.pos.y);

		if (posI.x < 0 || posI.y < 0 || posI.x >= vxs.GetLength(0) || posI.y >= vxs.GetLength(1))
		{
			return;
		}


		//It might be touching up to two surfaces: floor/ceiling and a wall.
		
		bool[] horzAndVert = new bool[2] { false, false };
		bool[] isLesserSurface = new bool[2];

		float horzDist = Mathf.Abs(drop.pos.x - (float)posI.x);
		if (horzDist < drop.radius)
		{
			horzAndVert[0] = true;
			isLesserSurface[0] = true;
		}
		else if (horzDist > (1.0f - drop.radius))
		{
			horzAndVert[0] = true;
			isLesserSurface[0] = false;
		}
		
		float vertDist = Mathf.Abs(drop.pos.y - (float)posI.y);
		if (vertDist < drop.radius)
		{
			horzAndVert[1] = true;
			isLesserSurface[1] = true;
		}
		else if (vertDist > (1.0f - drop.radius))
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
	/// Gets the size of the given Append Compute Buffer.
	/// </summary>
	private int GetBufferSize(ComputeBuffer buffer)
	{
		ComputeBuffer.CopyCount(buffer, countBuffer, 0);
		int[] valArray = new int[4] { 0, 0, 0, 0 };
		countBuffer.GetData(valArray);
		return valArray[0];
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

		WaterBurstShader.Dispatch(burstKernel, (nDrops / WorkSizeUpdate) + 1, 1, 1);
		NBursts += 1;
		NDrops += nDrops;
	}
}