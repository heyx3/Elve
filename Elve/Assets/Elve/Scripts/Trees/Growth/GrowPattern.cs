using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Defines a way for a tree to grow out.
/// </summary>
public interface IGrowPattern
{
	/// <summary>
	/// Gets a description of this  tree type for the UI.
	/// </summary>
	string TreeType { get; }


	/// <summary>
	/// Creates a new tree from a seed at the given position by modifying the given voxel grid.
	/// Returns some information about the tree's growth.
	/// Outputs into "changedPoses" all voxels that were changed.
	/// </summary>
	GrowData Sprout(VoxelTypes[,] worldGrid, VoxelTypes treeType, Vector2i seedPos,
					List<Vector2i> changedPoses);

	/// <summary>
	/// Grows out the given tree by modifying the given voxel grid it lives in.
	/// Returns all the voxels that were changed by this method.
	/// </summary>
	List<Vector2i> Grow(VoxelTypes[,] worldGrid, GrowData data);

	/// <summary>
	/// Finds whether the given world position is occupied by a part of this tree,
	/// optionally including the leaves.
	/// </summary>
	bool Occupies(GrowData baseData, Vector2i worldPos, bool includeLeaves);
}