using System;
using System.Collections.Generic;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


public class WorldTrees : MonoBehaviour
{
	public static WorldTrees Instance = null;


	[NonSerialized]
	public List<Tree> Trees = new List<Tree>();


	void Awake()
	{
		Assert.AreEqual(null, Instance, "More than one instance of 'WorldTrees' component!");

		Instance = this;
	}


	/// <summary>
	/// Returns null if no tree has a block at that position.
	/// </summary>
	public Tree GetTreeAt(Vector2i worldPos, bool includeLeaves)
	{
		for (int i = 0; i < Trees.Count; ++i)
		{
			if (Trees[i].IsInBoundingBox(worldPos) &&
				Trees[i].GrowPattern.Occupies(Trees[i].GrowDat, worldPos, includeLeaves))
			{
				return Trees[i];
			}
		}
		return null;
	}
}