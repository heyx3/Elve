using System;
using System.Collections.Generic;
using System.Linq;
using Assert = UnityEngine.Assertions.Assert;


//"Oak" trees grow straight upwards and have a bush of leaves along the top.


public class GrowData_Oak : GrowData
{
	/// <summary>
	/// The leaves on the tree.
	/// </summary>
	public List<Vector2i> Leaves;

	/// <summary>
	/// The Y coordinate of the highest wood block in the tree.
	/// </summary>
	public int TopY;

	/// <summary>
	/// The max distance a leaf can reach out from the tree top.
	/// 0 means they can only reach out one block.
	/// </summary>
	public int LeavesRadius = 0;


	public GrowData_Oak(Vector2i sproutPos, VoxelTypes treeType)
		: base(sproutPos, treeType, sproutPos, sproutPos)
	{
		Leaves = new List<Vector2i>();
		TopY = SproutPos.y;
	}
}

public class GrowPattern_Oak : IGrowPattern
{
	/// <summary>
	/// The number of blocks to grow upward each time the tree grows bigger.
	/// </summary>
	private const int GrowthRate = 3;


	public string TreeType { get { return "Oak"; } }

	public GrowData Sprout(VoxelTypes[,] worldGrid, VoxelTypes treeType, Vector2i seedPos,
						   List<Vector2i> changedPoses)
	{
		Assert.AreEqual(VoxelTypes.Empty, worldGrid[seedPos.x, seedPos.y],
						"Seed at " + seedPos + " is not in empty space");

		GrowData_Oak dat = new GrowData_Oak(seedPos, treeType);
		
		worldGrid[seedPos.x, seedPos.y] = treeType;
		changedPoses.Add(seedPos);
		dat.TopY = seedPos.y;

		//If we have room, add another tree block above it.
		if (seedPos.y < worldGrid.GetLength(1) - 1)
		{
			Vector2i above = seedPos.MoreY;
			if (WorldVoxels.IsTreeFodder(worldGrid[above.x, above.y]))
			{
				worldGrid[above.x, above.y] = treeType;
				changedPoses.Add(above);

				dat.TopY = above.y;
				dat.MaxBoundBox.y += 1;
			}
		}

		//Add a few leaves.
		AddLeaves(dat, worldGrid);
		changedPoses.Capacity += dat.Leaves.Count;
		changedPoses.AddRange(dat.Leaves);

		return dat;
	}

	public List<Vector2i> Grow(VoxelTypes[,] worldGrid, GrowData dataBase)
	{
		GrowData_Oak dat = (GrowData_Oak)dataBase;

		List<Vector2i> changed = new List<Vector2i>();

		//Clear out the current leaves.
		changed.Capacity += dat.Leaves.Count;
		for (int i = 0; i < dat.Leaves.Count; ++i)
		{
			worldGrid[dat.Leaves[i].x, dat.Leaves[i].y] = VoxelTypes.Empty;
			changed.Add(dat.Leaves[i]);
		}
		dat.Leaves.Clear();

		//Extend the tree-top upwards.
		Vector2i counter = new Vector2i(dat.SproutPos.x, dat.TopY);
		for (int i = 0; i < GrowthRate && counter.y < worldGrid.GetLength(1) - 1; ++i)
		{
			counter.y += 1;

			if (!WorldVoxels.IsTreeFodder(worldGrid[counter.x, counter.y]))
			{
				break;
			}

			worldGrid[counter.x, counter.y] = dat.TreeType;
			changed.Add(counter);
			dat.TopY = counter.y;
			dat.MaxBoundBox.y = UnityEngine.Mathf.Max(dat.MaxBoundBox.y, counter.y);
		}

		//Add leaves spreading out from the top.
		dat.LeavesRadius += 1;
		AddLeaves(dat, worldGrid);
		changed.Capacity += dat.Leaves.Count;
		changed.AddRange(dat.Leaves);

		return changed;
	}

	/// <summary>
	/// Adds leaves to the top of the given oak tree.
	/// </summary>
	private static void AddLeaves(GrowData_Oak dat, VoxelTypes[,] worldGrid)
	{
		Vector2i topPos = new Vector2i(dat.SproutPos.x, dat.TopY);
		if (topPos.x > 0)
		{
			AddLeaves_Iterate(topPos.LessX, dat.LeavesRadius, worldGrid, dat);
		}
		if (topPos.x < worldGrid.GetLength(0) - 1)
		{
			AddLeaves_Iterate(topPos.MoreX, dat.LeavesRadius, worldGrid, dat);
		}
		if (topPos.y < worldGrid.GetLength(1) - 1)
		{
			AddLeaves_Iterate(topPos.MoreY, dat.LeavesRadius, worldGrid, dat);
		}
	}
	/// <summary>
	/// The recursive loop for adding leaves to the top of a tree.
	/// </summary>
	private static void AddLeaves_Iterate(Vector2i pos, int stepsLeft,
										  VoxelTypes[,] worldGrid, GrowData_Oak dat)
	{
		if (WorldVoxels.IsTreeFodder(worldGrid[pos.x, pos.y]))
		{
			worldGrid[pos.x, pos.y] = VoxelTypes.Leaf;
			dat.Leaves.Add(pos);
			
			dat.MinBoundBox.x = UnityEngine.Mathf.Min(dat.MinBoundBox.x, pos.x);
			dat.MinBoundBox.y = UnityEngine.Mathf.Min(dat.MinBoundBox.y, pos.y);
			dat.MaxBoundBox.x = UnityEngine.Mathf.Max(dat.MaxBoundBox.x, pos.x);
			dat.MaxBoundBox.y = UnityEngine.Mathf.Max(dat.MaxBoundBox.y, pos.y);

			if (stepsLeft > 0)
			{
				stepsLeft -= 1;
				if (pos.x > 0)
				{
					AddLeaves_Iterate(pos.LessX, stepsLeft, worldGrid, dat);
				}
				if (pos.x < worldGrid.GetLength(0) - 1)
				{
					AddLeaves_Iterate(pos.MoreX, stepsLeft, worldGrid, dat);
				}
				if (pos.y > 0)
				{
					AddLeaves_Iterate(pos.LessY, stepsLeft, worldGrid, dat);
				}
				if (pos.y < worldGrid.GetLength(1) - 1)
				{
					AddLeaves_Iterate(pos.MoreY, stepsLeft, worldGrid, dat);
				}
			}
		}
	}


	public bool Occupies(GrowData baseData, Vector2i worldPos, bool includeLeaves)
	{
		//The logs occupy a single column of space.
		if (worldPos.x == baseData.SproutPos.x)
		{
			GrowData_Oak dat = (GrowData_Oak)baseData;
			if (worldPos.y >= dat.SproutPos.y && worldPos.y <= dat.TopY)
			{
				return true;
			}
			else
			{
				return includeLeaves && dat.Leaves.Any(le => (le == worldPos));
			}
		}
		else
		{
			return includeLeaves && ((GrowData_Oak)baseData).Leaves.Any(le => (le == worldPos));
		}
	}
}