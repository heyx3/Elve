﻿using System;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// Move the Elve from a floor/ceiling to a wall below/above it, or vice-versa.
/// </summary>
public class ElveState_CrossLedge : ElveState
{
	/// <summary>
	/// The amount of voxels to move. Should be +/- 1 in both axes.
	/// </summary>
	public Vector2i MoveAmount { get; private set; }


	/// <summary>
	/// If true, this state is currently playing the animation and not just moving into place.
	/// </summary>
	private bool actuallyRunning = false;
	private ElveBehavior.Surfaces targetSurface;


	public ElveState_CrossLedge(ElveBehavior owner, Vector2i moveAmount)
		: base(owner)
	{
		MoveAmount = moveAmount;

		Assert.IsTrue(moveAmount.x == 1 || moveAmount.x == -1);
		Assert.IsTrue(moveAmount.y == 1 || moveAmount.y == -1);
	}


	public override void OnStateStarting(ElveState oldState)
	{
		actuallyRunning = false;

		Vector3 pos = Owner.MyTransform.position;
		Vector2i posI = ToPosI(pos);
		
		float distToEdge;

		//See if we have to move to the edge of the voxel first.
		switch (Owner.CurrentSurface)
		{
			case ElveBehavior.Surfaces.Floor:
			case ElveBehavior.Surfaces.Ceiling:
				if (MoveAmount.x == -1)
				{
					distToEdge = pos.x - posI.x;
				}
				else
				{
					distToEdge = (posI.x + 1) - pos.x;
				}

				if (distToEdge > 0.001f)
				{
					Owner.CurrentState = new ElveState_MoveToEdge(Owner, this, MoveAmount.x);
					return;
				}
				break;

			case ElveBehavior.Surfaces.LeftWall:
			case ElveBehavior.Surfaces.RightWall:
				if (MoveAmount.y == -1)
				{
					distToEdge = pos.y - posI.y;
				}
				else
				{
					distToEdge = (posI.y + 1) - pos.y;
				}

				if (distToEdge > 0.001f)
				{
					Owner.CurrentState = new ElveState_MoveToEdge(Owner, this, MoveAmount.y);
					return;
				}
			break;

			default:
				Assert.IsTrue(false);
				break;
		}


		//Different but similar code will run based on which direction is being moved in.
		//Basically, for either of the 4 movement directions for ledge crossing,
		//    it can either be for a right-side-up ledge (from/to the floor)
		//    or an upside-down ledge (from/to the ceiling).
		ElveBehavior.Surfaces rightSideUpTargetSurface, upsideDownTargetSurface;
		ElveAnimStates rightSideUpAnim, upsideDownAnim;
		float xScale;
		if (MoveAmount.x == 1 && MoveAmount.y == 1)
		{
			rightSideUpTargetSurface = ElveBehavior.Surfaces.RightWall;
			upsideDownTargetSurface = ElveBehavior.Surfaces.Ceiling;
			rightSideUpAnim = ElveAnimStates.MountingLedge;
			upsideDownAnim = ElveAnimStates.DroppingToLedgeUpsideDown;
			xScale = 1.0f;
		}
		else if (MoveAmount.x == -1 && MoveAmount.y == 1)
		{
			rightSideUpTargetSurface = ElveBehavior.Surfaces.LeftWall;
			upsideDownTargetSurface = ElveBehavior.Surfaces.Ceiling;
			rightSideUpAnim = ElveAnimStates.MountingLedge;
			upsideDownAnim = ElveAnimStates.DroppingToLedgeUpsideDown;
			xScale = -1.0f;
		}
		else if (MoveAmount.x == 1 && MoveAmount.y == -1)
		{
			rightSideUpTargetSurface = ElveBehavior.Surfaces.Floor;
			upsideDownTargetSurface = ElveBehavior.Surfaces.RightWall;
			rightSideUpAnim = ElveAnimStates.DroppingToLedge;
			upsideDownAnim = ElveAnimStates.MountingLedgeUpsideDown;
			xScale = 1.0f;
		}
		else if (MoveAmount.x == -1 && MoveAmount.y == -1)
		{
			rightSideUpTargetSurface = ElveBehavior.Surfaces.Floor;
			upsideDownTargetSurface = ElveBehavior.Surfaces.LeftWall;
			rightSideUpAnim = ElveAnimStates.DroppingToLedge;
			upsideDownAnim = ElveAnimStates.MountingLedgeUpsideDown;
			xScale = -1.0f;
		}
		else
		{
			throw new InvalidOperationException("Should never see this! MoveAmount is " + MoveAmount);
		}


		//Get whether this is a right-side-up ledge or not.
		if (WorldVoxels.IsSolid(WorldVoxels.Instance.Voxels[posI.x + MoveAmount.x, posI.y]))
		{
			//Switch to the correct surface if needed.
			if (Owner.CurrentSurface != rightSideUpTargetSurface)
			{
				Owner.CurrentState = new ElveState_ChangeSurface(Owner, this,
																 Owner.CurrentSurface,
																 rightSideUpTargetSurface);
				return;
			}
			//Otherwise, everything is in place; play the animation.
			else
			{
				actuallyRunning = true;
				Owner.MyAnimator.AnimState = rightSideUpAnim;
			}
		}
		else
		{
			Assert.IsTrue(WorldVoxels.IsSolid(WorldVoxels.Instance.Voxels[posI.x, posI.y - 1]));
			
			//Switch to the correct surface if needed.
			if (Owner.CurrentSurface != upsideDownTargetSurface)
			{
				Owner.CurrentState = new ElveState_ChangeSurface(Owner, this,
																 Owner.CurrentSurface,
																 upsideDownTargetSurface);
				return;
			}
			//Otherwise, everything is in place; play the animation.
			else
			{
				actuallyRunning = true;
				Owner.MyAnimator.AnimState = upsideDownAnim;
			}
		}

		Owner.MyTransform.localScale = new Vector3(xScale, 1.0f, Owner.MyTransform.localScale.z);

		Owner.MyAnimator.ActiveAnim.GetComponent<ElveAnimFinishEvent>().OnAnimFinished += OnAnimFinished;
	}

	private void OnAnimFinished()
	{
		if (Owner.CurrentState == this)
		{
			//Bump the position forward into the next voxel.
			Vector3 myPos = Owner.MyTransform.position;
			const float dist = 0.05f;
			Owner.MyTransform.position += new Vector3(MoveAmount.x * dist, MoveAmount.y * dist, 0.0f);

			//Double-check that we actually went into the next voxel.
			Assert.IsTrue((int)Owner.MyTransform.position.x == ((int)myPos.x + MoveAmount.x));
			Assert.IsTrue((int)Owner.MyTransform.position.y == ((int)myPos.y + MoveAmount.y));

			Owner.CurrentSurface = targetSurface;

			Success();
		}
	}


	public override void Update() { }
}