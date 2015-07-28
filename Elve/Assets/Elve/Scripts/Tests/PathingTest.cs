using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(Camera))]
public class PathingTest : MonoBehaviour
{
	public ElveMovementController Elve = null;

	private Camera cam;

	private Vector2 pos1, pos2;
	private List<VoxelNode> path = new List<VoxelNode>();


	void Start()
	{
		cam = GetComponent<Camera>();

		pos1 = new Vector2(0.0f, 0.0f);
		pos2 = new Vector2(0.0f, 0.0f);
		path = new List<VoxelNode>();
		path.Add(new VoxelNode(null, new Vector2i(0, 0), new Vector2i(0, 0)));
	}
	void Update()
	{
		Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);

		worldPos.x = Mathf.Max(0.0f, worldPos.x);
		worldPos.y = Mathf.Max(0.0f, worldPos.y);

		if (Input.GetMouseButtonDown(0))
		{
			pos1 = new Vector2((int)worldPos.x, (int)worldPos.y);
			CalcPath();
		}
		else if (Input.GetMouseButtonDown(1))
		{
			pos2 = new Vector2((int)worldPos.x, (int)worldPos.y);
			CalcPath();
		}
	}

	private void CalcPath()
	{
		PathFinder<VoxelNode> pather = new PathFinder<VoxelNode>(VoxelGraph.Instance, VoxelEdge.MakeEdge);

		pather.Start = new VoxelNode(new Vector2i((int)pos1.x, (int)pos1.y));
		pather.End = new VoxelNode(new Vector2i((int)pos2.x, (int)pos2.y));
		pather.FindPath();

		path = pather.CurrentPath.ToList();

		if (Elve != null)
		{
			Elve.StartPath(pos2);
		}
	}


	void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawSphere(new Vector3(pos1.x + 0.5f, pos1.y + 0.5f, 0.0f), 0.25f);

		Gizmos.color = new Color(0.75f, 0.75f, 0.75f);
		for (int i = 0; i < path.Count - 1; ++i)
		{
			Gizmos.DrawLine(new Vector3(path[i].WorldPos.x + 0.5f, path[i].WorldPos.y + 0.5f, 0.0f),
							new Vector3(path[i + 1].WorldPos.x + 0.5f, path[i + 1].WorldPos.y + 0.5f, 0.0f));
		}

		Gizmos.color = new Color(0.25f, 0.25f, 0.25f);
		Gizmos.DrawSphere(new Vector3(pos2.x + 0.5f, pos2.y + 0.5f, 0.0f), 0.25f);

		
		//Draw any connections from the moused-over voxel.
		if (WorldVoxels.Instance != null && WorldVoxels.Instance.Connections != null)
		{
			Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
			worldPos.x = Mathf.Clamp(worldPos.x, 0.0f, (float)WorldVoxels.Instance.Voxels.GetLength(0) - 0.001f);
			worldPos.y = Mathf.Clamp(worldPos.y, 0.0f, (float)WorldVoxels.Instance.Voxels.GetLength(1) - 0.001f);

			Vector2i worldPosI = new Vector2i((int)worldPos.x, (int)worldPos.y);
			WorldVoxels.VoxelConnections conn =
				WorldVoxels.Instance.Connections[worldPosI.x, worldPosI.y];
			
			Vector3 centerPos = new Vector3(worldPosI.x + 0.5f, worldPosI.y + 0.5f, 0.0f);

			Gizmos.color = new Color(1.0f, 0.0f, 1.0f, 0.5f);
			Gizmos.DrawSphere(centerPos , 0.25f);
			
			if (conn.CanWalkLeft)
				Gizmos.DrawLine(centerPos, centerPos + new Vector3(-1, 0, 0));
			if (conn.CanWalkRight)
				Gizmos.DrawLine(centerPos, centerPos + new Vector3(1, 0, 0));
			if (conn.CanClimbDown)
				Gizmos.DrawLine(centerPos, centerPos + new Vector3(0, -1, 0));
			if (conn.CanClimbUp)
				Gizmos.DrawLine(centerPos, centerPos + new Vector3(0, 1, 0));
			if (conn.CanMoveDownLeft)
				Gizmos.DrawLine(centerPos, centerPos + new Vector3(-1, -1, 0));
			if (conn.CanMoveDownRight)
				Gizmos.DrawLine(centerPos, centerPos + new Vector3(1, -1, 0));
			if (conn.CanMoveUpLeft)
				Gizmos.DrawLine(centerPos, centerPos + new Vector3(-1, 1, 0));
			if (conn.CanMoveUpRight)
				Gizmos.DrawLine(centerPos, centerPos + new Vector3(1, 1, 0));
		}
	}
}