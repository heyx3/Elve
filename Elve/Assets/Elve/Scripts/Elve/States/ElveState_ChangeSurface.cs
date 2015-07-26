using System;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// Changes from one surface to another,
/// assuming the Elve is at the corner where both surfaces meet.
/// </summary>
public class ElveState_ChangeSurface : ElveState
{
	public ElveBehavior.Surfaces From { get; private set; }
	public ElveBehavior.Surfaces To { get; private set; }


	/// <summary>
	/// The state that will take over after this one is finished.
	/// </summary>
	public ElveState ToResume = null;

	
	public ElveState_ChangeSurface(ElveBehavior owner, ElveState toResume,
								   ElveBehavior.Surfaces from, ElveBehavior.Surfaces to)
		: base(owner)
	{
		From = from;
		To = to;
		ToResume = toResume;
	}

	public override void OnStateStarting(ElveState oldState)
	{
		//Enable the correct animation.
		float xScale = Owner.MyTransform.localScale.x;
		switch (From)
		{
			case ElveBehavior.Surfaces.Floor:
				switch (To)
				{
					case ElveBehavior.Surfaces.Ceiling:
						Owner.MyAnimator.AnimState = ElveAnimStates.FloorToCeiling;
						break;
					case ElveBehavior.Surfaces.LeftWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.FloorToWall;
						xScale = -1.0f;
						break;
					case ElveBehavior.Surfaces.RightWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.FloorToWall;
						xScale = 1.0f;
						break;
					default:
						Assert.IsTrue(false);
						break;
				}
				break;
			case ElveBehavior.Surfaces.Ceiling:
				switch (To)
				{
					case ElveBehavior.Surfaces.Floor:
						Owner.MyAnimator.AnimState = ElveAnimStates.CeilingToFloor;
						break;
					case ElveBehavior.Surfaces.LeftWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.CeilingToWall;
						xScale = -1.0f;
						break;
					case ElveBehavior.Surfaces.RightWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.CeilingToWall;
						xScale = 1.0f;
						break;
					default:
						Assert.IsTrue(false);
						break;
				}
				break;
			case ElveBehavior.Surfaces.LeftWall:
				switch (To)
				{
					case ElveBehavior.Surfaces.Ceiling:
						Owner.MyAnimator.AnimState = ElveAnimStates.CeilingToWall;
						xScale = -1.0f;
						break;
					case ElveBehavior.Surfaces.Floor:
						Owner.MyAnimator.AnimState = ElveAnimStates.FloorToWall;
						xScale = -1.0f;
						break;
					case ElveBehavior.Surfaces.RightWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.WallToWall;
						xScale = -1.0f;
						break;
					default:
						Assert.IsTrue(false);
						break;
				}
				break;
			case ElveBehavior.Surfaces.RightWall:
				switch (To)
				{
					case ElveBehavior.Surfaces.Ceiling:
						Owner.MyAnimator.AnimState = ElveAnimStates.CeilingToWall;
						xScale = 1.0f;
						break;
					case ElveBehavior.Surfaces.Floor:
						Owner.MyAnimator.AnimState = ElveAnimStates.FloorToWall;
						xScale = 1.0f;
						break;
					case ElveBehavior.Surfaces.LeftWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.WallToWall;
						xScale = 1.0f;
						break;
					default:
						Assert.IsTrue(false);
						break;
				}
				break;
		}
		Owner.MyTransform.localScale = new Vector3(xScale, Owner.MyTransform.localScale.y,
												   Owner.MyTransform.localScale.z);
		
		//When the animation is finished, switch states.
		Owner.MyAnimator.ActiveAnim.GetComponent<ElveAnimFinishEvent>().OnAnimFinished += OnAnimFinished;
	}


	public override void Update() { }


	private void OnAnimFinished()
	{
		if (Owner.CurrentState == this)
		{
			Owner.CurrentSurface = To;
			Owner.CurrentState = ToResume;
		}
	}
}