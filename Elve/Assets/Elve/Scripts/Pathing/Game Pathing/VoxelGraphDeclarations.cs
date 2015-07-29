using System;
using System.Collections.Generic;
using MovementTypes = PathingConstants.MovementTypes;


public class VoxelNode : Node
{
	public Vector2i WorldPos;


	public VoxelTypes BlockType
	{
		get { return WorldVoxels.Instance.Voxels[WorldPos.x, WorldPos.y]; }
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


	public VoxelNode(Vector2i worldPos)
	{
		WorldPos = worldPos;
	}


	public override bool IsEqualTo(Node other)
	{
		VoxelNode vn = (VoxelNode)other;
		return WorldPos == vn.WorldPos;
	}
	public override bool IsNotEqualTo(Node other)
	{
		VoxelNode vn = (VoxelNode)other;
		return WorldPos != vn.WorldPos;
	}

	public override int GetHashCode()
	{
		return WorldPos.GetHashCode();
	}
}

public class VoxelEdge : Edge<VoxelNode>
{
	/// <summary>
	/// Makes an edge with an arbitrary movement type.
	/// Used in the construction of a PathFinder instance.
	/// </summary>
	public static VoxelEdge MakeEdge(VoxelNode start, VoxelNode end)
	{
		return new VoxelEdge(start, end, MovementTypes.Walk);
	}


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


/// <summary>
/// Is a singleton simply because it doesn't have any state and it's a waste to keep re-allocating.
/// </summary>
public class VoxelGraph : Graph<VoxelNode>
{
	public static VoxelGraph Instance = new VoxelGraph();


	private VoxelGraph() { }


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
								   MovementTypes.Climb));
		}
		if (connections.CanClimbUp)
		{
			ends.Add(new VoxelEdge(start, new VoxelNode(start.WorldPos.MoreY),
								   MovementTypes.Climb));
		}

		if (connections.CanMoveDownLeft)
		{
			ends.Add(new VoxelEdge(start, new VoxelNode(start.WorldPos + new Vector2i(-1, -1)),
								   MovementTypes.Ledge));
		}
		if (connections.CanMoveDownRight)
		{
			ends.Add(new VoxelEdge(start, new VoxelNode(start.WorldPos + new Vector2i(1, -1)),
								   MovementTypes.Ledge));
		}
		
		if (connections.CanMoveUpLeft)
		{
			ends.Add(new VoxelEdge(start, new VoxelNode(start.WorldPos + new Vector2i(-1, 1)),
								   MovementTypes.Ledge));
		}
		if (connections.CanMoveUpRight)
		{
			ends.Add(new VoxelEdge(start, new VoxelNode(start.WorldPos + new Vector2i(1, 1)),
								   MovementTypes.Ledge));
		}
	}
}