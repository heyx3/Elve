using System;
using System.Collections.Generic;
using MovementTypes = PathingConstants.MovementTypes;


public class VoxelNode : Node
{
	public Chunk Chnk;
	public Vector2i LocalPos, WorldPos;


	public VoxelTypes BlockType
	{
		get { return Chnk.Grid[LocalPos.x, LocalPos.y]; }
	}

	public bool IsLeftEdgeOfLevel { get { return WorldPos.x == 0; } }
	public bool IsRightEdgeOfLevel
	{
		get { return WorldPos.x == Chunk.Size * WorldVoxels.Instance.Chunks.GetLength(0); }
	}
	public bool IsBottomEdgeOfLevel { get { return WorldPos.y == 0; } }
	public bool IsTopEdgeOfLevel
	{
		get { return WorldPos.y == Chunk.Size * WorldVoxels.Instance.Chunks.GetLength(1); }
	}


	public VoxelNode(Chunk chnk, Vector2i localPos, Vector2i worldPos)
	{
		Chnk = chnk;
		LocalPos = localPos;
		WorldPos = worldPos;
	}
	public VoxelNode(Vector2i worldPos)
	{
		Chnk = WorldVoxels.Instance.GetChunkAt(worldPos);
		LocalPos = worldPos - Chnk.MinCorner;
		WorldPos = worldPos;
	}


	public override bool IsEqualTo(Node other)
	{
		VoxelNode vn = (VoxelNode)other;
		return Chnk == vn.Chnk && LocalPos == vn.LocalPos;
	}
	public override bool IsNotEqualTo(Node other)
	{
		VoxelNode vn = (VoxelNode)other;
		return Chnk != vn.Chnk || LocalPos != vn.LocalPos;
	}

	public override int GetHashCode()
	{
		return WorldPos.GetHashCode();
	}
}

public class VoxelEdge : Edge<VoxelNode>
{
	public MovementTypes MoveType;


	public VoxelEdge(VoxelNode start, VoxelNode end, MovementTypes type)
		: base(start, end) { MoveType = type; }


	public override float GetTraversalCost(PathFinder<VoxelNode> pather)
	{
		//If searching for a specific end, use the distance to it as an A* heuristic.
		float heuristic = 0.0f;
		if (pather.HasSpecificEnd)
		{
			int x = End.WorldPos.x - pather.End.WorldPos.x,
				y = End.WorldPos.y - pather.End.WorldPos.y;
			heuristic = (float)((x * x) + (y * y));
		}

		return PathingConstants.Instance.CostForMovements[(int)MoveType].Cost + heuristic;
	}
	public override float GetSearchCost(PathFinder<VoxelNode> pather)
	{
		return PathingConstants.Instance.CostForMovements[(int)MoveType].Cost;
	}
}

public class VoxelGraph : Graph<VoxelNode>
{
	public void GetConnections(VoxelNode start, List<Edge<VoxelNode>> ends)
	{
		WorldVoxels.VoxelConnections connections =
			WorldVoxels.Instance.Connections[start.WorldPos.x, start.WorldPos.y];

		if (connections.CanWalkLeft)
		{
			ends.Add(new VoxelEdge(start, new VoxelNode(start.WorldPos.LessX),
								   MovementTypes.Walk));
		}
		if (connections.CanWalkRight)
		{
			ends.Add(new VoxelEdge(start, new VoxelNode(start.WorldPos.MoreX),
								   MovementTypes.Walk));
		}

		if (connections.CanClimbDown)
		{
			ends.Add(new VoxelEdge(start, new VoxelNode(start.WorldPos.LessY),
								   MovementTypes.ClimbWall));
		}
		if (connections.CanClimbUp)
		{
			ends.Add(new VoxelEdge(start, new VoxelNode(start.WorldPos.MoreY),
								   MovementTypes.ClimbWall));
		}

		if (connections.CanDropLedgeLeft)
		{
			ends.Add(new VoxelEdge(start, new VoxelNode(start.WorldPos + new Vector2i(-1, -1)),
								   MovementTypes.DropDownFromLedge));
		}
		if (connections.CanDropLedgeRight)
		{
			ends.Add(new VoxelEdge(start, new VoxelNode(start.WorldPos + new Vector2i(1, -1)),
								   MovementTypes.DropDownFromLedge));
		}
		
		if (connections.CanClimbLedgeLeft)
		{
			ends.Add(new VoxelEdge(start, new VoxelNode(start.WorldPos + new Vector2i(-1, 1)),
								   MovementTypes.ClimbOverLedge));
		}
		if (connections.CanClimbLedgeRight)
		{
			ends.Add(new VoxelEdge(start, new VoxelNode(start.WorldPos + new Vector2i(1, 1)),
								   MovementTypes.ClimbOverLedge));
		}
	}
}