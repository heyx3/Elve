using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Tests the ability to add/remove voxels at will.
/// </summary>
public class VoxelAddRemoveTest : MonoBehaviour
{
	void Update()
	{
		if (Input.GetMouseButtonDown(2))
		{
			Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			
			if (worldPos.x < 0.0f || worldPos.y < 0.0f)
			{
				return;
			}

			VoxelTypes newType = CycleVoxel(WorldVoxels.Instance.GetVoxelAt(new Vector2(worldPos.x, worldPos.y)));
			WorldVoxels.Instance.SetVoxelAt(worldPos, newType);
		}
	}

	private VoxelTypes CycleVoxel(VoxelTypes inV)
	{
		return (WorldVoxels.IsSolid(inV) ? VoxelTypes.Empty : VoxelTypes.HardRock);
	}
}