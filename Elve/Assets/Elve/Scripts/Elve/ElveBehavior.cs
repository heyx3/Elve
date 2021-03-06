﻿using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The FSM controlling Elve movement/behavior.
/// </summary>
[RequireComponent(typeof(ElveAnimController))]
public class ElveBehavior : MonoBehaviour
{
	/// <summary>
	/// The different surfaces an Elve can cling to inside an empty voxel.
	/// </summary>
	public enum Surfaces
	{
		Floor,
		Ceiling,
		LeftWall,
		RightWall,
	}


	/// <summary>
	/// The current state. Can be set to "null" if the Elve should just stand still.
	/// </summary>
	public ElveState CurrentState
	{
		get { return currentState; }
		set
		{
			if (currentState == value)
			{
				return;
			}

			if (currentState != null)
			{
				currentState.OnStateEnding(value);
			}

			ElveState old = currentState;
			currentState = value;
			if (currentState != null)
			{
				currentState.OnStateStarting(old);
			}
		}
	}
	private ElveState currentState = null;

	/// <summary>
	/// The surface this Elve is currently holding onto.
	/// </summary>
	public Surfaces CurrentSurface = Surfaces.Floor;
	

	public Transform MyTransform { get; private set; }
	public ElveAnimController MyAnimator { get; private set; }


	void Awake()
	{
		MyTransform = transform;
		MyAnimator = GetComponent<ElveAnimController>();
	}
	void Update()
	{
		if (CurrentState != null)
		{
			CurrentState.Update();
		}
		else
		{
			switch (CurrentSurface)
			{
				case Surfaces.Floor:
					MyAnimator.AnimState = ElveAnimStates.IdleFloor;
					break;
				case Surfaces.LeftWall:
				case Surfaces.RightWall:
					MyAnimator.AnimState = ElveAnimStates.IdleWall;
					break;
				case Surfaces.Ceiling:
					MyAnimator.AnimState = ElveAnimStates.IdleCeiling;
					break;

				default: throw new InvalidOperationException();
			}
		}
	}

	
	/// <summary>
	/// An empty callback. Used above when triggering the "OnStateChanged" event.
	/// </summary>
	private static void DummyFunc(ElveBehavior m, ElveState s1, ElveState s2) { }
}