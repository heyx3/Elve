using System;
using UnityEngine;


public class ElveAnimController : MonoBehaviour
{
	/// <summary>
	/// Used to simplify the Inspector interface.
	/// </summary>
	[Serializable]
	public class StateAndObject
	{
		public ElveAnimStates State;
		public GameObject Object = null;

		public StateAndObject(ElveAnimStates state) { State = state; }
	}


	/// <summary>
	/// The current animation state.
	/// </summary>
	public ElveAnimStates AnimState
	{
		get { return animState; }
		set
		{
			if (animState != value)
			{
				animState = value;
				for (int i = 0; i < AnimChildren.Length; ++i)
				{
					if (AnimChildren[i].State == animState)
					{
						AnimChildren[i].Object.SetActive(true);
						ActiveAnim = AnimChildren[i].Object;
					}
					else
					{
						AnimChildren[i].Object.SetActive(false);
					}
				}
			}
		}
	}
	private ElveAnimStates animState = ElveAnimStates.MountingLedge;

	public GameObject ActiveAnim { get; private set; }


	/// <summary>
	/// The various child objects displaying various Elve animations.
	/// They will be activated/deactivated as necessary when animation state changes.
	/// </summary>
	public StateAndObject[] AnimChildren = new StateAndObject[]
	{
		new StateAndObject(ElveAnimStates.IdleFloor),
		new StateAndObject(ElveAnimStates.IdleWall),
		new StateAndObject(ElveAnimStates.IdleCeiling),

		new StateAndObject(ElveAnimStates.Walking),
		new StateAndObject(ElveAnimStates.ClimbingWall),
		new StateAndObject(ElveAnimStates.ClimbingCeiling),

		new StateAndObject(ElveAnimStates.MountingLedge),
		new StateAndObject(ElveAnimStates.DroppingToLedge),
		new StateAndObject(ElveAnimStates.MountingLedgeUpsideDown),
		new StateAndObject(ElveAnimStates.DroppingToLedgeUpsideDown),

		new StateAndObject(ElveAnimStates.FloorToWall),
		new StateAndObject(ElveAnimStates.FloorToCeiling),
		new StateAndObject(ElveAnimStates.WallToFloor),
		new StateAndObject(ElveAnimStates.WallToWall),
		new StateAndObject(ElveAnimStates.WallToCeiling),
		new StateAndObject(ElveAnimStates.CeilingToFloor),
		new StateAndObject(ElveAnimStates.CeilingToWall),
	};


	void Start()
	{
		//Make sure all animations are accounted for.
		for (int i = 0; i < AnimChildren.Length; ++i)
		{
			if (AnimChildren[i].Object == null)
			{
				Debug.LogError("Animation child object for '" + AnimChildren[i].State + "' is null!");
			}
		}
		foreach (ElveAnimStates state in Enum.GetValues(typeof(ElveAnimStates)))
		{
			bool found = false;
			for (int i = 0; i < AnimChildren.Length; ++i)
			{
				if (AnimChildren[i].State == state)
				{
					found = true;
					break;
				}
			}

			if (!found)
			{
				Debug.LogError("Couldn't find animation object for '" + state + "'!");
			}
		}

		AnimState = ElveAnimStates.IdleFloor;
	}
}