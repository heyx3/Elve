using System;
using System.Collections.Generic;
using UnityEngine;


public class Tree
{
	public IGrowPattern GrowPattern;
	public GrowData GrowDat;

	/// <summary>
	/// The amount of water this tree has absorbed in it.
	/// </summary>
	public float Water = 0.0f;

	/// <summary>
	/// Whether this tree is allowed to grow organically once it has enough water.
	/// </summary>
	public bool AllowGrowth = true;
	/// <summary>
	/// If true, Elves will pull material out of this tree as needed.
	/// </summary>
	public bool UseAsResource = false;
	/// <summary>
	/// If true, Elves will pull water out of this tree as needed.
	/// </summary>
	public bool UseAsWaterSource = false;


	public Tree(IGrowPattern pattern, GrowData dat)
	{
		GrowPattern = pattern;
		GrowDat = dat;
	}


	public bool IsInBoundingBox(Vector2i pos)
	{
		return (pos.x >= GrowDat.MinBoundBox.x && pos.y >= GrowDat.MinBoundBox.y &&
				pos.x <= GrowDat.MaxBoundBox.x && pos.y <= GrowDat.MaxBoundBox.y);
	}

	/// <summary>
	/// Grows the tree and recalculates pathing/regenerates meshes for any voxels that changed.
	/// </summary>
	public void Grow()
	{
		WorldVoxels.Instance.UpdateVoxelsAt(GrowPattern.Grow(WorldVoxels.Instance.Voxels, GrowDat));
	}
	/// <summary>
	/// Grows the tree but does NOT recalculate secondary data such as pathing.
	/// Only used when first generating the world.
	/// </summary>
	public void GrowBareBones()
	{
		GrowPattern.Grow(WorldVoxels.Instance.Voxels, GrowDat);
	}
}