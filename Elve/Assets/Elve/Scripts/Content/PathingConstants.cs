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
	/// to get from one block to an adjacent (orthogonal or diagonal) block.
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
		ClimbWall,

		/// <summary>
		/// Climbing up and over a wall to stand on top of it. Moves up and to the left/right.
		/// </summary>
		ClimbOverLedge,
		/// <summary>
		/// The opposite of "ClimbOverLedge": drop down to the wall on the side of the floor.
		/// Moves down and to the left/right.
		/// </summary>
		DropDownFromLedge,


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
			new MovementTypeAndCost(MovementTypes.ClimbWall, 1.0f),
			new MovementTypeAndCost(MovementTypes.ClimbOverLedge, 1.0f),
			new MovementTypeAndCost(MovementTypes.DropDownFromLedge, 1.0f),
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