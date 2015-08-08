using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//Provides the various context menu items when clicking on a voxel.


/// <summary>
/// Plant a seed in empty space.
/// </summary>
public struct ContextMenuItem_PlantSeed : IContextMenuItem<Vector2i>
{
	public string Text { get { return "Plant Seed"; } }


	public bool IsItemAvailable(Vector2i voxelPos)
	{
		VoxelTypes[,] vxs = WorldVoxels.Instance.Voxels;

		//The space must be empty and touching a plantable surface.
		return vxs[voxelPos.x, voxelPos.y] == VoxelTypes.Empty &&
			   ((voxelPos.x > 0 || WorldVoxels.CanPlantOn(vxs[voxelPos.x - 1, voxelPos.y])) ||
			    (voxelPos.x < vxs.GetLength(0) - 1 &&
				 WorldVoxels.CanPlantOn(vxs[voxelPos.x + 1, voxelPos.y])) ||
				(voxelPos.y == 0 || WorldVoxels.CanPlantOn(vxs[voxelPos.x, voxelPos.y - 1])) ||
			    (voxelPos.y < vxs.GetLength(1) - 1 &&
				 WorldVoxels.CanPlantOn(vxs[voxelPos.x, voxelPos.y + 1])));
	}
	public void OnSelected(Vector2i voxelPos)
	{
		//TODO: Bring up "choose seed" context menu.
	}
}