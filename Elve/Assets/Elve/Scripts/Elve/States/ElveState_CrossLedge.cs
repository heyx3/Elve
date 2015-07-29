using System;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// Moves the Elve from a floor/ceiling to a nearby voxel's wall (or vice-versa).
/// </summary>
public class ElveState_CrossLedge : ElveState
{
	/// <summary>
	/// The amount of voxels to move. Should be +/- 1 in both axes.
	/// </summary>
	public Vector2i MoveAmount { get; private set; }


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
		Vector2i testPos = posI;
		ElveBehavior.Surfaces rightSideUpTargetSurface, upsideDownTargetSurface,
							  rightSideUpFinalSurface, upsideDownFinalSurface;
		ElveAnimStates rightSideUpAnim, upsideDownAnim;
		float xScale;
		if (MoveAmount.x == 1 && MoveAmount.y == 1)
		{
			testPos.x += 1;
			
			rightSideUpTargetSurface = ElveBehavior.Surfaces.RightWall;
			rightSideUpAnim = ElveAnimStates.MountingLedge;
			rightSideUpFinalSurface = ElveBehavior.Surfaces.Floor;

			upsideDownTargetSurface = ElveBehavior.Surfaces.Ceiling;
			upsideDownAnim = ElveAnimStates.DroppingToLedgeUpsideDown;
			upsideDownFinalSurface = ElveBehavior.Surfaces.LeftWall;

			xScale = 1.0f;
		}
		else if (MoveAmount.x == -1 && MoveAmount.y == 1)
		{
			testPos.x -= 1;

			rightSideUpTargetSurface = ElveBehavior.Surfaces.LeftWall;
			rightSideUpAnim = ElveAnimStates.MountingLedge;
			rightSideUpFinalSurface = ElveBehavior.Surfaces.Floor;

			upsideDownTargetSurface = ElveBehavior.Surfaces.Ceiling;
			upsideDownAnim = ElveAnimStates.DroppingToLedgeUpsideDown;
			upsideDownFinalSurface = ElveBehavior.Surfaces.RightWall;

			xScale = -1.0f;
		}
		else if (MoveAmount.x == 1 && MoveAmount.y == -1)
		{
			testPos.y -= 1;

			rightSideUpTargetSurface = ElveBehavior.Surfaces.Floor;
			rightSideUpAnim = ElveAnimStates.DroppingToLedge;
			rightSideUpFinalSurface = ElveBehavior.Surfaces.LeftWall;

			upsideDownTargetSurface = ElveBehavior.Surfaces.RightWall;
			upsideDownAnim = ElveAnimStates.MountingLedgeUpsideDown;
			upsideDownFinalSurface = ElveBehavior.Surfaces.Ceiling;

			xScale = -1.0f;
		}
		else if (MoveAmount.x == -1 && MoveAmount.y == -1)
		{
			testPos.y -= 1;

			rightSideUpTargetSurface = ElveBehavior.Surfaces.Floor;
			rightSideUpAnim = ElveAnimStates.DroppingToLedge;
			rightSideUpFinalSurface = ElveBehavior.Surfaces.RightWall;

			upsideDownTargetSurface = ElveBehavior.Surfaces.LeftWall;
			upsideDownAnim = ElveAnimStates.MountingLedgeUpsideDown;
			upsideDownFinalSurface = ElveBehavior.Surfaces.Ceiling;

			xScale = 1.0f;
		}
		else
		{
			throw new InvalidOperationException("Should never see this! MoveAmount is " + MoveAmount);
		}


		//Get whether this is a right-side-up ledge or not.
		if (WorldVoxels.IsSolid(WorldVoxels.Instance.Voxels[testPos.x, testPos.y]))
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
				Owner.MyAnimator.AnimState = rightSideUpAnim;
				targetSurface = rightSideUpFinalSurface;
			}
		}
		else
		{
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
				Owner.MyAnimator.AnimState = upsideDownAnim;
				targetSurface = upsideDownFinalSurface;
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
			Assert.IsTrue((int)Owner.MyTransform.position.x == ((int)myPos.x + MoveAmount.x),
						  "X Pos is " + Owner.MyTransform.position.x + "; prev pos is " + myPos.x);
			Assert.IsTrue((int)Owner.MyTransform.position.y == ((int)myPos.y + MoveAmount.y),
						  "Y Pos is " + Owner.MyTransform.position.y + "; prev pos is " + myPos.y);

			Owner.CurrentSurface = targetSurface;

			Success();
		}
	}


	public override void Update() { }
}