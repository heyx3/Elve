using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A singleton GameObject script containing all necessary constants about pathing.
/// Note that this object will not be destroyed by scene changes.
/// </summary>
public class PathingConstants : MonoBehaviour
{
	/// <summary>
	/// The different atomic types of movement an Elve can do
	/// to get from one block to an adjacent (orthogonal) block.
	/// </summary>
	public enum MovementTypes
	{
		/// <summary>
		/// Walking along the floor. Moves left or right.
		/// Includes moving along the ceiling, which is technically climbing.
		/// </summary>
		Walk = 0,
		/// <summary>
		/// Climbing along the side of a wall. Moves upward or downward.
		/// </summary>
		Climb,

		/// <summary>
		/// Climbing over a ledge.
		/// Moves left/right for half a block and up/down for half a block,
		/// plus a little time for crossing the ledge itself.
		/// </summary>
		Ledge,


		NumberOfMovementTypes,
	}


	public static PathingConstants Instance = null;


	[Serializable]
	public class MovementTypeAndCost
	{
		public MovementTypes Type;
		public float Cost;
		public MovementTypeAndCost(MovementTypes type, float cost) { Type = type; Cost = cost; }
	}

	/// <summary>
	/// The pathing cost of each type of movement an Elve can make.
	/// </summary>
	public MovementTypeAndCost[] CostForMovements =
		new MovementTypeAndCost[(int)MovementTypes.NumberOfMovementTypes]
		{
			new MovementTypeAndCost(MovementTypes.Walk, 1.0f),
			new MovementTypeAndCost(MovementTypes.Climb, 1.0f),
			new MovementTypeAndCost(MovementTypes.Ledge, 1.1f)
		};


	void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("An instance of 'PathingConstants' already exists!");
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		//Verify sanity of "CostForMovements" values.
		for (int i = 0; i < (int)MovementTypes.NumberOfMovementTypes; ++i)
		{
			if ((MovementTypes)i != CostForMovements[i].Type)
			{
				Debug.LogError("Element " + i + " of 'CostForMovements' should have type " +
							   ((MovementTypes)i) + " but has type " + CostForMovements[i].Type);
			}
		}
	}
}