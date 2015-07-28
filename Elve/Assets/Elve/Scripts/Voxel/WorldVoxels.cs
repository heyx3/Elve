using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A singleton that stores the world voxel data.
/// Stores the chunks on a 2D grid as well as the full world voxel grid for easy reference when pathing.
/// </summary>
public class WorldVoxels : MonoBehaviour
{
	/// <summary>
	/// Defines what types of movement can be done from a certain voxel to its adjacent ones.
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


	public static bool IsSolid(VoxelTypes type)
	{
		switch (type)
		{
			case VoxelTypes.Dirt:
			case VoxelTypes.SoftRock:
			case VoxelTypes.HardRock:
				return true;

			case VoxelTypes.Empty:
				return false;

			default:
				throw new InvalidOperationException("Unknown voxel type " + type);
		}
	}


	public static WorldVoxels Instance;


	public Chunk[,] Chunks = null;
	public VoxelTypes[,] Voxels = null;
	public VoxelConnections[,] Connections = null;



	void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("More than one instance of 'WorldVoxels' script at a time!");
			Destroy(this);
			return;
		}
		Instance = this;
	}


	/// <summary>
	/// Should be called once all the Chunks are generated and stored in the "Chunks" array.
	/// Sets up the "Voxels" and "Connections" arrays.
	/// </summary>
	public void GenerateVoxelGrid()
	{
		Voxels = new VoxelTypes[Chunks.GetLength(0) * Chunk.Size,
								Chunks.GetLength(1) * Chunk.Size];
		for (int y = 0; y < Voxels.GetLength(1); ++y)
		{
			for (int x = 0; x < Voxels.GetLength(0); ++x)
			{
				Chunk chnk = Chunks[x / Chunk.Size, y / Chunk.Size];
				Voxels[x, y] = chnk.Grid[x % Chunk.Size, y % Chunk.Size];
			}
		}

		//Calculate connections.
		Connections = new VoxelConnections[Voxels.GetLength(0), Voxels.GetLength(1)];
		for (int y = 0; y < Voxels.GetLength(1); ++y)
		{
			for (int x = 0; x < Voxels.GetLength(0); ++x)
			{
				GetConnections(new Vector2i(x, y));
			}
		}
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
	/// Gets the chunk at the given world position.
	/// Assumes the given position is inside the world's voxel grid.
	/// </summary>
	public Chunk GetChunkAt(Vector2 worldPos)
	{
		Vector2i worldPosI = new Vector2i((int)worldPos.x, (int)worldPos.y),
				 chunkPosI = new Vector2i(worldPosI.x / Chunk.Size,
										  worldPosI.y / Chunk.Size);
		return Chunks[chunkPosI.x, chunkPosI.y];
	}
	/// <summary>
	/// Gets the chunk at the given world position.
	/// Assumes the given position is inside the world's voxel grid.
	/// </summary>
	public Chunk GetChunkAt(Vector2i worldPos)
	{
		return Chunks[worldPos.x / Chunk.Size, worldPos.y / Chunk.Size];
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
	/// Automatically regenerates the chunk's mesh and updates pathing connections.
	/// </summary>
	public void SetVoxelAt(Vector2 worldPos, VoxelTypes newType)
	{
		Vector2i worldPosI = new Vector2i((int)worldPos.x, (int)worldPos.y),
				 chunkPosI = new Vector2i(worldPosI.x / Chunk.Size,
										  worldPosI.y / Chunk.Size);

		Chunks[chunkPosI.x, chunkPosI.y].Grid[worldPosI.x - (chunkPosI.x * Chunk.Size),
											  worldPosI.y - (chunkPosI.y * Chunk.Size)] = newType;
		Chunks[chunkPosI.x, chunkPosI.y].RegenMesh();

		Voxels[worldPosI.x, worldPosI.y] = newType;


		//Re-calculate connections for adjacent voxels as well as this one.
		for (int y = -1; y <= 1; ++y)
		{
			for (int x = -1; x <= 1; ++x)
			{
				GetConnections(new Vector2i(worldPosI.x + x, worldPosI.y + y));
			}
		}
	}
}