using System;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// Moves the Elve to one of the edges of the voxel it's currently occupying.
/// </summary>
public class ElveState_MoveToEdge : ElveState
{
	/// <summary>
	/// Indicates the direction this Elve is moving in.
	/// Horizontal if on the floor/ceiling; vertical if on a wall.
	/// -1 is left or down; 1 is right or up.
	/// </summary>
	public float Dir { get; private set; }

	/// <summary>
	/// The state that takes over once this state is done.
	/// </summary>
	public ElveState ToResume { get; private set; }

	/// <summary>
	/// The target position along the movement axis.
	/// </summary>
	private int targetPos;


	public ElveState_MoveToEdge(ElveBehavior owner, ElveState toResume, float dir)
		: base(owner)
	{
		Dir = dir;
		ToResume = toResume;
	}

	public override void OnStateStarting(ElveState oldState)
	{
		Vector3 myPos = Owner.MyTransform.position;

		switch (Owner.CurrentSurface)
		{
			case ElveBehavior.Surfaces.Floor:
			case ElveBehavior.Surfaces.Ceiling:
				targetPos = (int)myPos.x;
				targetPos += (Dir > 0.0f) ? 1 : 0;
				break;

			case ElveBehavior.Surfaces.LeftWall:
			case ElveBehavior.Surfaces.RightWall:
				targetPos = (int)myPos.y;
				targetPos += (Dir > 0.0f) ? 1 : 0;
				break;
		}
	}

	public override void Update()
	{
		Vector3 myPos = Owner.MyTransform.position;
		float moveAmount, distToTarget;

		switch (Owner.CurrentSurface)
		{
			case ElveBehavior.Surfaces.Floor:
			case ElveBehavior.Surfaces.Ceiling:
				
				//If on the floor, make sure the floor is solid.
				Assert.IsTrue(Owner.CurrentSurface == ElveBehavior.Surfaces.Ceiling ||
							  ((int)Owner.MyTransform.position.y == 0 ||
							   WorldVoxels.IsSolid(WorldVoxels.Instance.Voxels[(int)Owner.MyTransform.position.x,
																			   (int)Owner.MyTransform.position.y - 1])));
				//If on the ceiling, make sure the ceiling is solid.
				Assert.IsTrue(Owner.CurrentSurface == ElveBehavior.Surfaces.Floor ||
							  ((int)Owner.MyTransform.position.y < WorldVoxels.Instance.Voxels.GetLength(1) - 1 &&
							    WorldVoxels.IsSolid(WorldVoxels.Instance.Voxels[(int)Owner.MyTransform.position.x,
																			    (int)Owner.MyTransform.position.y + 1])));

				//Animate.
				Owner.MyAnimator.AnimState = (Owner.CurrentSurface == ElveBehavior.Surfaces.Floor ?
												ElveAnimStates.Walking :
												ElveAnimStates.ClimbingCeiling);
				Owner.MyTransform.localScale = new Vector3(Dir, 1.0f, Owner.MyTransform.localScale.z);

				//Move.
				moveAmount = Time.deltaTime * ElveConstants.Instance.WalkSpeed;
				distToTarget = Mathf.Abs(myPos.x - targetPos);
				//If close to the goal, jump there and end the state.
				if (moveAmount > distToTarget)
				{
					Owner.MyTransform.position = new Vector3((float)targetPos - (Dir * 0.0001f), myPos.y,
															 myPos.z);
					Success(ToResume);
				}
				else
				{
					myPos.x += moveAmount * Dir;
					Owner.MyTransform.position = myPos;
				}
				break;

			case ElveBehavior.Surfaces.LeftWall:
			case ElveBehavior.Surfaces.RightWall:
				
				//If on the left wall, make sure it's solid.
				Assert.IsTrue(Owner.CurrentSurface == ElveBehavior.Surfaces.RightWall ||
							  ((int)Owner.MyTransform.position.x > 0 &&
							   WorldVoxels.IsSolid(WorldVoxels.Instance.Voxels[(int)Owner.MyTransform.position.x - 1,
																			   (int)Owner.MyTransform.position.y])));
				//If on the right wall, make sure it's solid.
				Assert.IsTrue(Owner.CurrentSurface == ElveBehavior.Surfaces.LeftWall ||
							  ((int)Owner.MyTransform.position.x < WorldVoxels.Instance.Voxels.GetLength(0) - 1 &&
							   WorldVoxels.IsSolid(WorldVoxels.Instance.Voxels[(int)Owner.MyTransform.position.x + 1,
																			   (int)Owner.MyTransform.position.y])));

				//Animate.
				Owner.MyAnimator.AnimState = ElveAnimStates.ClimbingWall;
				Owner.MyTransform.localScale = new Vector3((Owner.CurrentSurface == ElveBehavior.Surfaces.LeftWall ?
																-1.0f : 1.0f),
														   Dir, Owner.MyTransform.localScale.z);

				//Move.
				moveAmount = Time.deltaTime * ElveConstants.Instance.ClimbSpeed;
				distToTarget = Mathf.Abs(myPos.y - targetPos);
				if (moveAmount > distToTarget)
				{
					Owner.MyTransform.position = new Vector3(myPos.x, (float)targetPos - (Dir * 0.0001f),
															 myPos.z);
					Success(ToResume);
				}
				else
				{
					myPos.y += moveAmount * Dir;
					Owner.MyTransform.position = myPos;
				}
				break;

			default:
				Assert.IsTrue(false);
				break;
		}
	}
}