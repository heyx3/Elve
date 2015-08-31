using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Manages the various seeds that have yet to grow.
/// </summary>
public class SeedManager : Singleton<SeedManager>
{
	public struct SeedData
	{
		public Vector2i Pos;
		public IGrowPattern GrowthPattern;

		public VoxelTypes SeedType { get { return WorldVoxels.GetVoxelAt(Pos); } }

		public SeedData(Vector2i pos, IGrowPattern growthPattern)
		{
			Pos = pos;
			GrowthPattern = growthPattern;
		}
	}


	public List<SeedData> Seeds = new List<SeedData>();

	private int checkIndex = 0;


	void Update()
	{
		//Every frame, pick a seed and check it out.
		if (Seeds.Count > 0)
		{
			checkIndex %= Seeds.Count;
			SeedData seed = Seeds[checkIndex];
			VoxelTypes ground = WorldVoxels.GetVoxelAt(seed.Pos.LessY);

			//If something happened to the ground, destroy this seed.
			if (!WorldVoxels.CanPlantOn(ground))
			{
				Seeds.RemoveAt(checkIndex);
			}
			//Otherwise, if the ground is wet enough, grow into a tree.
			else if (WorldVoxels.Instance.Wetness[seed.Pos.x, seed.Pos.y - 1] >=
					 TreeConstants.Instance.MinSeedWetness)
			{
				List<Vector2i> changedPoses = new List<Vector2i>();
				GrowData dat = seed.GrowthPattern.Sprout(WorldVoxels.Instance.Voxels,
														 WorldVoxels.ConvertSeedTypeTreeType(seed.SeedType),
														 seed.Pos, changedPoses);

				WorldVoxels.Instance.UpdateVoxelsAt(changedPoses);

				WorldTrees.Instance.Trees.Add(new Tree(seed.GrowthPattern, dat));
				Seeds.RemoveAt(checkIndex);
			}
			//Otherwise, just move to the next seed to check.
			else
			{
				checkIndex += 1;
			}
		}
	}
}