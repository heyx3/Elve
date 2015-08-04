using System;
using System.Collections.Generic;
using UnityEngine;


public class Tree
{
	public IGrowPattern GrowPattern;
	public GrowData GrowDat;


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
		List<Vector2i> changed = GrowPattern.Grow(WorldVoxels.Instance.Voxels, GrowDat);

		//Recalculate pathing stuff.		
		for (int i = 0; i < changed.Count; ++i)
			WorldVoxels.Instance.GetConnections(changed[i]);

		//Regenerate meshes.
		List<Chunk> alreadyDone = new List<Chunk>();
		for (int i = 0; i < changed.Count; ++i)
		{
			Chunk chnk = WorldVoxels.Instance.Chunks[changed[i].x / Chunk.Size,
													 changed[i].y / Chunk.Size];
			if (!alreadyDone.Contains(chnk))
			{
				chnk.RegenMesh();
				alreadyDone.Add(chnk);
			}
		}
	}
	/// <summary>
	/// Grows the tree but does NOT recalculate pathing or regenerate meshes for affected voxels.
	/// Should generally only be used when first generating the world.
	/// </summary>
	public void GrowBareBones()
	{
		GrowPattern.Grow(WorldVoxels.Instance.Voxels, GrowDat);
	}
}