using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// A singleton that stores and renders the world voxel data.
/// </summary>
public class WorldVoxels : MonoBehaviour
{
	#region Voxel property lookup tables

	private readonly static bool[] isSolid = new bool[(int)VoxelTypes.Empty + 1]
	{
		true, true, true, //soft rock, hard rock, dirt
		true, true, false, //wooden tree, leaf, tree background
		false, //wood seed
		false, //empty
	};
	private readonly static bool[] isTree = new bool[(int)VoxelTypes.Empty + 1]
	{
		false, false, false, //soft rock, hard rock, dirt
		true, false, false, //wooden tree, leaf, tree background
		false, //wood seed
		false, //empty
	};
	private readonly static bool[] isItem = new bool[(int)VoxelTypes.Empty + 1]
	{
		false, false, false, //soft rock, hard rock, dirt
		false, false, false, //wooden tree, leaf, tree background
		true, //wood seed
		false, //empty
	};
	private readonly static bool[] isTreeFodder = new bool[(int)VoxelTypes.Empty + 1]
	{
		false, false, false, //soft rock, hard rock, dirt
		false, false, false, //wooden tree, leaf, tree background
		false, //wood seed
		true, //empty
	};
	private readonly static bool[] canPlantIn = new bool[(int)VoxelTypes.Empty + 1]
	{
		false, false, false, //soft rock, hard rock, dirt
		false, false, false, //wooden tree, leaf, tree background
		false, //wood seed
		true, //empty
	};
	private readonly static bool[] canPlantOn = new bool[(int)VoxelTypes.Empty + 1]
	{
		true, true, true, //soft rock, hard rock, dirt
		false, false, false, //wooden tree, leaf, tree background
		false, //wood seed
		false, //empty
	};
	private readonly static VoxelTypes[] seedTreeConverter = new VoxelTypes[(int)VoxelTypes.Empty + 1]
	{
		VoxelTypes.Empty, VoxelTypes.Empty, VoxelTypes.Empty, //soft rock, hard rock, dirt
		VoxelTypes.Item_WoodSeed, //wooden tree
		VoxelTypes.Empty, VoxelTypes.Empty, //leaf, tree background
		VoxelTypes.Tree_Wood, //wood seed
		VoxelTypes.Empty, //empty
	};

	#endregion

	/// <summary>
	/// Whether the given voxel is a solid surface.
	/// </summary>
	public static bool IsSolid(VoxelTypes type) { return isSolid[(int)type]; }
	/// <summary>
	/// Whether the given voxel is a tree block (not including leaves).
	/// </summary>
	public static bool IsTree(VoxelTypes type) { return isTree[(int)type]; }
	/// <summary>
	/// Whether the given voxel is an item, like a planted seed.
	/// </summary>
	public static bool IsItem(VoxelTypes type) { return isItem[(int)type]; }
	/// <summary>
	/// Whether a tree can destroy/grow over the given type of block.
	/// </summary>
	public static bool IsTreeFodder(VoxelTypes type) { return isTreeFodder[(int)type]; }
	/// <summary>
	/// Whether the given voxel can have seeds planted inside it.
	/// </summary>
	public static bool CanPlantIn(VoxelTypes type) { return canPlantIn[(int)type]; }
	/// <summary>
	/// Whether the given voxel can have seeds planted on its outside surface.
	/// </summary>
	public static bool CanPlantOn(VoxelTypes type) { return canPlantOn[(int)type]; }
	/// <summary>
	/// Converts between seed types and their corresponding tree types.
	/// Returns "Empty" if the given type isn't a seed or a tree.
	/// </summary>
	public static VoxelTypes ConvertSeedTypeTreeType(VoxelTypes type)
	{
		return seedTreeConverter[(int)type];
	}

	public static VoxelTypes GetVoxelAt(Vector2i pos) { return Instance.Voxels[pos.x, pos.y]; }

	
	/// <summary>
	/// Defines what types of movement can be performed from a certain voxel to its adjacent ones.
	/// </summary>
	public struct VoxelConnections
	{
		public uint Bitflag;

		public void ClearAll() { Bitflag = 0; }

		public bool CanWalkLeft        { get { return (Bitflag & 1) == 1; } }
		public bool CanWalkRight       { get { return (Bitflag & 2) == 2; } }
		public bool CanClimbUp         { get { return (Bitflag & 4) == 4; } }
		public bool CanClimbDown       { get { return (Bitflag & 8) == 8; } }
		public bool CanMoveUpLeft      { get { return (Bitflag & 16) == 16; } }
		public bool CanMoveUpRight     { get { return (Bitflag & 32) == 32; } }
		public bool CanMoveDownLeft    { get { return (Bitflag & 64) == 64; } }
		public bool CanMoveDownRight   { get { return (Bitflag & 128) == 128; } }
		
		public void Set_WalkLeft()        { Bitflag |= 1; }
		public void Set_WalkRight()       { Bitflag |= 2; }
		public void Set_ClimbUp()         { Bitflag |= 4; }
		public void Set_ClimbDown()       { Bitflag |= 8; }
		public void Set_MoveUpLeft()      { Bitflag |= 16; }
		public void Set_MoveUpRight()     { Bitflag |= 32; }
		public void Set_MoveDownLeft()    { Bitflag |= 64; }
		public void Set_MoveDownRight()   { Bitflag |= 128; }
	}


	/// <summary>
	/// Gets the color for the given block in the voxel texture.
	/// </summary>
	public static Color GetVoxelTexValue(VoxelTypes block)
	{
		bool solid = IsSolid(block);
		return new Color(solid ? 1.0f : 0.0f,
						 solid ? WaterConstants.Instance.Friction[(int)block].Friction : 0.0f,
						 0.0f, 0.0f);
	}

	/// <summary>
	/// Gets whether the given position is a valid position in the voxel grid.
	/// </summary>
	public static bool IsValidPos(Vector2i voxelPos)
	{
		return (voxelPos.x >= 0 && voxelPos.y >= 0 &&
				voxelPos.x < Instance.Voxels.GetLength(0) &&
				voxelPos.y < Instance.Voxels.GetLength(1));
	}


	public static WorldVoxels Instance;

	
	public Material VoxelBlockMat;

	/// <summary>
	/// Every voxel block's information, stored on a 2D texture. Used for water simulation.
	/// The Red value is 0.0 if not solid, and 1.0 if solid.
	/// The Green value is the coefficient of friction for the solid blocks.
	/// </summary>
	public Texture2D VoxelTex;

	/// <summary>
	/// The voxel grid.
	/// </summary>
	public VoxelTypes[,] Voxels = null;
	/// <summary>
	/// Contains a square region of meshes for the voxel grid.
	/// </summary>
	public Chunk[,] Chunks = null;
	/// <summary>
	/// The connections from each voxel to its 8 neighbors.
	/// </summary>
	public VoxelConnections[,] Connections = null;
	/// <summary>
	/// The wetness of each voxel, from 0 to 1.
	/// </summary>
	public float[,] Wetness = null;

	private Vector2 vMin, vMax;
	private Transform tr;


	void Awake()
	{
		tr = transform;

		if (Instance != null)
		{
			Debug.LogError("More than one instance of 'WorldVoxels' script at a time!");
			Destroy(this);
			return;
		}
		Instance = this;
	}
	void OnRenderObject()
	{
		Camera cam = Camera.current;

		//Don't render if this camera doesn't see this object's layer.
		if ((cam.cullingMask & (1 << gameObject.layer)) == 0)
		{
			return;
		}

		//Figure out which chunks are in view and render them.

		Vector2 orthoHalf = new Vector2(cam.orthographicSize * cam.pixelWidth / cam.pixelHeight,
										cam.orthographicSize);
		Vector3 camPos = cam.transform.position;
		Vector2 viewMin = new Vector2(camPos.x - orthoHalf.x, camPos.y - orthoHalf.y),
				viewMax = viewMin + (orthoHalf * 2.0f);

		Vector2i viewMinI = new Vector2i((int)viewMin.x, (int)viewMin.y),
				 viewMaxI = new Vector2i((int)viewMax.x + 1, (int)viewMax.y + 1);

		Vector2i chunkMinI = viewMinI / Chunk.Size,
				 chunkMaxI = viewMaxI / Chunk.Size;

		if (cam.gameObject.name == "PathingTest")
		{
			vMin = viewMin;
			vMax = viewMax;
		}

		

		VoxelBlockMat.SetPass(0);
		Matrix4x4 worldM = tr.localToWorldMatrix;
		for (int y = chunkMinI.y; y <= chunkMaxI.y && y < Chunks.GetLength(1); ++y)
		{
			if (y > 0)
			{
				for (int x = chunkMinI.x; x <= chunkMaxI.x && x < Chunks.GetLength(0); ++x)
				{
					if (x > 0)
					{
						Graphics.DrawMeshNow(Chunks[x, y].VoxelMesh, worldM);
					}
				}
			}
		}
	}
	void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere(new Vector3(vMin.x, vMin.y, 0.0f), 0.25f);
		Gizmos.DrawSphere(new Vector3(vMax.x, vMax.y, 0.0f), 0.25f);
	}


	/// <summary>
	/// Should be called once the "Voxels" grid is filled with its initial values.
	/// Generates all the extra voxel data used for pathing, rendering, etc.
	/// </summary>
	public void GenerateSecondaryData()
	{
		UnityEngine.Assertions.Assert.IsTrue(Voxels.GetLength(0) % Chunk.Size == 0 &&
											  Voxels.GetLength(1) % Chunk.Size == 0,
											 "Voxel grid size " + Voxels.GetLength(0) +
											  "x" + Voxels.GetLength(1) +
											  " is not divisible by chunk size " + Chunk.Size);

		//Calculate chunks.
		Vector2i nChunks = new Vector2i(Voxels.GetLength(0) / Chunk.Size,
										Voxels.GetLength(1) / Chunk.Size);
		Chunks = new Chunk[nChunks.x, nChunks.y];
		for (int y = 0; y < nChunks.y; ++y)
			for (int x = 0; x < nChunks.x; ++x)
				Chunks[x, y] = new Chunk(new Vector2i(x, y));

		//Calculate connections.
		Connections = new VoxelConnections[Voxels.GetLength(0), Voxels.GetLength(1)];
		for (int y = 0; y < Voxels.GetLength(1); ++y)
			for (int x = 0; x < Voxels.GetLength(0); ++x)
				GetConnections(new Vector2i(x, y));

		//Set up the texture.
		VoxelTex = new Texture2D(Voxels.GetLength(0), Voxels.GetLength(1), TextureFormat.RGHalf,
								 false, true);
		VoxelTex.name = "Voxel Data Texture";
		VoxelTex.anisoLevel = 0;
		VoxelTex.filterMode = FilterMode.Point;
		VoxelTex.wrapMode = TextureWrapMode.Clamp;
		Color[] cols = new Color[VoxelTex.width * VoxelTex.height];
		for (int y = 0; y < VoxelTex.height; ++y)
			for (int x = 0; x < VoxelTex.width; ++x)
				cols[(y * VoxelTex.width) + x] = GetVoxelTexValue(Voxels[x, y]);
		VoxelTex.SetPixels(cols);
		VoxelTex.Apply();

		//Set up the wetness data.
		Wetness = new float[Voxels.GetLength(0), Voxels.GetLength(1)];
		for (int y = 0; y < Wetness.GetLength(1); ++y)
			for (int x = 0; x < Wetness.GetLength(0); ++x)
				Wetness[x, y] = 0.0f;
	}

	/// <summary>
	/// Recalculates connections for the given voxel (in world coordinates).
	/// </summary>
	public void GetConnections(Vector2i voxel)
	{
		int x = voxel.x,
			y = voxel.y;

		bool isLeftEdge = (x == 0),
			 isRightEdge = (x == Voxels.GetLength(0) - 1),
			 isBottomEdge = (y == 0),
			 isTopEdge = (y == Voxels.GetLength(1) - 1);

		Connections[x, y].ClearAll();

		//If this is an empty space, find connections.
		if (!IsSolid(Voxels[x, y]))
		{
			//Left side connections.
			if (!isLeftEdge)
			{
				if (IsSolid(Voxels[x - 1, y]))
				{
					//See if we can climb straight up.
					if (!isTopEdge && !IsSolid(Voxels[x, y + 1]) && IsSolid(Voxels[x - 1, y + 1]))
					{
						Connections[x, y].Set_ClimbUp();
					}
					//See if we can climb straight down.
					if (!isBottomEdge && !IsSolid(Voxels[x, y - 1]) && IsSolid(Voxels[x - 1, y - 1]))
					{
						Connections[x, y].Set_ClimbDown();
					}
					//See if we can mount the ledge right-side-up.
					if (!isTopEdge && !IsSolid(Voxels[x, y + 1]) && !IsSolid(Voxels[x - 1, y + 1]))
					{
						Connections[x, y].Set_MoveUpLeft();
					}
					//See if we can mount the ledge upside-down.
					if (!isBottomEdge && !IsSolid(Voxels[x, y - 1]) && !IsSolid(Voxels[x - 1, y - 1]))
					{
						Connections[x, y].Set_MoveDownLeft();
					}
				}
				else
				{
					//If the top/bottom edge is solid towards the left,
					//    we can walk or climb along the ceiling.
					if ((!isTopEdge && IsSolid(Voxels[x, y + 1]) && IsSolid(Voxels[x - 1, y + 1])) ||
						(isBottomEdge || (IsSolid(Voxels[x, y - 1]) && IsSolid(Voxels[x - 1, y - 1]))))
					{
						Connections[x, y].Set_WalkLeft();
					}
					//See if we can mount the upside-down ledge.
					if (!isTopEdge && IsSolid(Voxels[x, y + 1]) && !IsSolid(Voxels[x - 1, y + 1]))
					{
						Connections[x, y].Set_MoveUpLeft();
					}
					//See if we can drop down a ledge.
					if (!isBottomEdge && IsSolid(Voxels[x, y - 1]) && !IsSolid(Voxels[x - 1, y - 1]))
					{
						Connections[x, y].Set_MoveDownLeft();
					}
				}
			}
			//Right side connections.
			if (!isRightEdge)
			{
				if (IsSolid(Voxels[x + 1, y]))
				{
					//See if we can climb straight up.
					if (!isTopEdge && !IsSolid(Voxels[x, y + 1]) && IsSolid(Voxels[x + 1, y + 1]))
					{
						Connections[x, y].Set_ClimbUp();
					}
					//See if we can climb straight down.
					if (!isBottomEdge && !IsSolid(Voxels[x, y - 1]) && IsSolid(Voxels[x + 1, y - 1]))
					{
						Connections[x, y].Set_ClimbDown();
					}
					//See if we can mount the ledge right-side-up.
					if (!isTopEdge && !IsSolid(Voxels[x, y + 1]) && !IsSolid(Voxels[x + 1, y + 1]))
					{
						Connections[x, y].Set_MoveUpRight();
					}
					//See if we can mount the ledge upside-down.
					if (!isBottomEdge && !IsSolid(Voxels[x, y - 1]) && !IsSolid(Voxels[x + 1, y - 1]))
					{
						Connections[x, y].Set_MoveDownRight();
					}
				}
				else
				{
					//If the top/bottom edge is solid towards the right,
					//    we can walk or climb along the ceiling.
					if ((!isTopEdge && IsSolid(Voxels[x, y + 1]) && IsSolid(Voxels[x + 1, y + 1])) ||
						(isBottomEdge || (IsSolid(Voxels[x, y - 1]) && IsSolid(Voxels[x + 1, y - 1]))))
					{
						Connections[x, y].Set_WalkRight();
					}
					//See if we can mount the upside-down ledge.
					if (!isTopEdge && IsSolid(Voxels[x, y + 1]) && !IsSolid(Voxels[x + 1, y + 1]))
					{
						Connections[x, y].Set_MoveUpRight();
					}
					//See if we can drop down a ledge.
					if (!isBottomEdge && IsSolid(Voxels[x, y - 1]) && !IsSolid(Voxels[x + 1, y - 1]))
					{
						Connections[x, y].Set_MoveDownRight();
					}
				}
			}
		}
	}

	/// <summary>
	/// Gets the voxel at the given world position.
	/// Assumes the given position is inside the world's voxel grid.
	/// </summary>
	public VoxelTypes GetVoxelAt(Vector2 worldPos)
	{
		Vector2i worldPosI = new Vector2i((int)worldPos.x, (int)worldPos.y);
		return Voxels[worldPosI.x, worldPosI.y];
	}
	/// <summary>
	/// Sets the given world position to contain the given block.
	/// Assumes the given position is inside the world's voxel grid.
	/// Automatically updates secondary data like meshes, jobs, and pathing.
	/// </summary>
	public void SetVoxelAt(Vector2i worldPos, VoxelTypes newValue)
	{
		Vector2i chunkPos = new Vector2i(worldPos.x / Chunk.Size,
										 worldPos.y / Chunk.Size);
		
		Voxels[worldPos.x, worldPos.y] = newValue;
		Chunks[chunkPos.x, chunkPos.y].RegenMesh();

		VoxelTex.SetPixel(worldPos.x, worldPos.y, GetVoxelTexValue(newValue));
		VoxelTex.Apply();

		//Re-calculate connections for adjacent voxels as well as this one.
		for (int y = -1; y <= 1; ++y)
		{
			for (int x = -1; x <= 1; ++x)
			{
				GetConnections(new Vector2i(worldPos.x + x, worldPos.y + y));
			}
		}

		JobManager.Instance.OnBlockChanged(worldPos, newValue);

		//If the voxel is being empied out, squeeze out all the water.
		if (!IsSolid(newValue))
		{
			BurstWaterFromVoxel(worldPos);
		}
	}

	/// <summary>
	/// Updates secondary data for the given batch of changed voxels.
	/// </summary>
	public void UpdateVoxelsAt(List<Vector2i> changedPoses)
	{
		List<Chunk> alreadyDone = new List<Chunk>();

		for (int i = 0; i < changedPoses.Count; ++i)
		{
			VoxelTypes voxel = GetVoxelAt(changedPoses[i]);

			//Pathing.
			GetConnections(changedPoses[i]);

			//Mesh.
			Chunk chnk = Chunks[changedPoses[i].x / Chunk.Size,
								changedPoses[i].y / Chunk.Size];
			if (!alreadyDone.Contains(chnk))
			{
				chnk.RegenMesh();
				alreadyDone.Add(chnk);
			}

			//Water.
			if (!IsSolid(voxel))
			{
				BurstWaterFromVoxel(changedPoses[i]);
			}

			//GPU data.
			VoxelTex.SetPixel(changedPoses[i].x, changedPoses[i].y, GetVoxelTexValue(voxel));
		}
		VoxelTex.Apply();
	}

	/// <summary>
	/// Empties out all water from the given voxel.
	/// Sets its "wetness" to 0 and bursts some water drops based on how wet it was.
	/// </summary>
	public void BurstWaterFromVoxel(Vector2i worldPos)
	{
		float wetness = Wetness[worldPos.x, worldPos.y];
		Wetness[worldPos.x, worldPos.y] = 0.0f;

		float maxDrops = 1.0f / WaterConstants.Instance.DropWetness;
		int nDrops = (int)Mathf.Ceil(Mathf.Lerp(0.0f, maxDrops, wetness) - 0.00001f);

		Vector2 burstDist = new Vector2(WaterConstants.Instance.VoxelBurstDist,
										WaterConstants.Instance.VoxelBurstDist);
		Vector2 minPos = new Vector2(worldPos.x + 0.5f, worldPos.y + 0.5f) - burstDist,
				maxPos = minPos + (2.0f * burstDist),
				minSpeed = -Vector2.one.normalized * WaterConstants.Instance.VoxelBurstSpeed,
				maxSpeed = -minSpeed;
		WaterController.Instance.BurstDrops(minPos, maxPos, minSpeed, maxSpeed,
											WaterConstants.Instance.VoxelBurstRadiusMin,
											WaterConstants.Instance.VoxelBurstRadiusMax,
											nDrops);
	}
}