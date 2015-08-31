using System;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


public class TreeConstants : Singleton<TreeConstants>
{
	/// <summary>
	/// The number of blocks upward an Oak tree can grow each time it grows bigger.
	/// </summary>
	public int OakGrowthRate = 3;

	/// <summary>
	/// The minimum "wetness" needed for a seed to grow into a tree.
	/// </summary>
	public float MinSeedWetness = 0.75f;


	protected override void Awake()
	{
		base.Awake();

		Assert.IsTrue(OakGrowthRate >= 0, "Oak growth rate must be at least 0");
		Assert.IsTrue(MinSeedWetness >= 0.0f, "Min seed wetness must be at least 0.0");
	}
}