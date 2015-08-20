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
}