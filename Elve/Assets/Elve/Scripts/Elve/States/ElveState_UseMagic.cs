using System;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// Makes the Elve shoot magic at a surface for a certain amount of time.
/// </summary>
public class ElveState_UseMagic : ElveState
{
	/// <summary>
	/// The amount of time left until this state is completed.
	/// </summary>
	public float TimeLeft { get; set; }

	/// <summary>
	/// The surface the magic is being applied to.
	/// </summary>
	public ElveBehavior.Surfaces Target { get; private set; }


	public ElveState_UseMagic(ElveBehavior owner, float duration, ElveBehavior.Surfaces target)
		: base(owner)
	{
		TimeLeft = duration;
		Target = target;
	}


	public override void OnStateStarting(ElveState oldState)
	{
		//Enable the correct animation and set the x/y scale.
		float xScale = Owner.MyTransform.localScale.x,
			  yScale = Owner.MyTransform.localScale.y;
		switch (Owner.CurrentSurface)
		{
			case ElveBehavior.Surfaces.Floor:
				yScale = 1.0f;
				switch (Target)
				{
					case ElveBehavior.Surfaces.Floor:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicFloorToFloor;
						break;
					case ElveBehavior.Surfaces.Ceiling:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicFloorToCeiling;
						break;
					case ElveBehavior.Surfaces.LeftWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicFloorToWall;
						xScale = -1.0f;
						break;
					case ElveBehavior.Surfaces.RightWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicFloorToWall;
						xScale = 1.0f;
						break;
					default:
						Assert.IsTrue(false);
						break;
				}
				break;

			case ElveBehavior.Surfaces.Ceiling:
				yScale = 1.0f;
				switch (Target)
				{
					case ElveBehavior.Surfaces.Floor:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicCeilingToFloor;
						break;
					case ElveBehavior.Surfaces.Ceiling:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicCeilingToCeiling;
						break;
					case ElveBehavior.Surfaces.LeftWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicCeilingToWall;
						xScale = -1.0f;
						break;
					case ElveBehavior.Surfaces.RightWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicCeilingToWall;
						xScale = 1.0f;
						break;
					default:
						Assert.IsTrue(false);
						break;
				}
				break;

			case ElveBehavior.Surfaces.LeftWall:
				xScale = 1.0f;
				switch (Target)
				{
					case ElveBehavior.Surfaces.Floor:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicWallToCeiling;
						yScale = -1.0f;
						break;
					case ElveBehavior.Surfaces.Ceiling:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicWallToCeiling;
						yScale = 1.0f;
						break;
					case ElveBehavior.Surfaces.LeftWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicWallToSelf;
						break;
					case ElveBehavior.Surfaces.RightWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicWallToWall;
						break;
					default:
						Assert.IsTrue(false);
						break;
				}
				break;

			case ElveBehavior.Surfaces.RightWall:
				xScale = -1.0f;
				switch (Target)
				{
					case ElveBehavior.Surfaces.Floor:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicWallToCeiling;
						yScale = -1.0f;
						break;
					case ElveBehavior.Surfaces.Ceiling:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicWallToCeiling;
						yScale = 1.0f;
						break;
					case ElveBehavior.Surfaces.LeftWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicWallToWall;
						break;
					case ElveBehavior.Surfaces.RightWall:
						Owner.MyAnimator.AnimState = ElveAnimStates.UseMagicWallToSelf;
						break;
					default:
						Assert.IsTrue(false);
						break;
				}
				break;

			default:
				Assert.IsTrue(false);
				break;
		}
		Owner.MyTransform.localScale = new Vector3(xScale, yScale, Owner.MyTransform.localScale.z);
	}

	public override void Update()
	{
		TimeLeft -= WorldTime.DeltaTime;

		if (TimeLeft <= 0.0f)
		{
			Success(null);
		}
	}
}