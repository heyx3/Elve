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


	public ElveState_MoveToEdge(ElveBehavior owner, ElveState toResume, float dir)
		: base(owner)
	{
		Dir = dir;
		ToResume = toResume;
	}
	
	public override void Update()
	{
		Vector3 myPos;
		int targetX, targetY;

		switch (Owner.CurrentSurface)
		{
			case ElveBehavior.Surfaces.Floor:

				Assert.IsTrue((int)Owner.MyTransform.position.y == 0 ||
							  WorldVoxels.IsSolid(WorldVoxels.Instance.Voxels[(int)Owner.MyTransform.position.x,
																			  (int)Owner.MyTransform.position.y - 1]));

				//Animate.
				Owner.MyAnimator.AnimState = ElveAnimStates.Walking;
				Owner.MyTransform.localScale = new Vector3(Dir, 1.0f, Owner.MyTransform.localScale.z);

				//Figure out the target position to walk to.
				myPos = Owner.MyTransform.position;
				targetX = (int)myPos.x;
				if (Dir > 0.0f)
				{
					targetX += 1;
				}

				//Walk.
				myPos.x += Dir * Time.deltaTime * ElveConstants.Instance.WalkSpeed;
				Owner.MyTransform.position = myPos;

				//If we've reached our destination, switch states.
				if ((Dir < 0.0f && myPos.x < targetX) ||
					(Dir > 0.0f && myPos.x == targetX))
				{
					Owner.MyTransform.position = new Vector3((float)targetX - (Dir * 0.01f), myPos.y,
															 myPos.z);
					Owner.CurrentState = ToResume;
				}
				break;

			case ElveBehavior.Surfaces.Ceiling:

				Assert.IsTrue((int)Owner.MyTransform.position.y < WorldVoxels.Instance.Voxels.GetLength(1) - 1 &&
							  WorldVoxels.IsSolid(WorldVoxels.Instance.Voxels[(int)Owner.MyTransform.position.x,
																			  (int)Owner.MyTransform.position.y + 1]));

				//Animate.
				Owner.MyAnimator.AnimState = ElveAnimStates.ClimbingCeiling;
				Owner.MyTransform.localScale = new Vector3(Dir, 1.0f, Owner.MyTransform.localScale.z);

				//Figure out the target position to walk to.
				myPos = Owner.MyTransform.position;
				targetX = (int)myPos.x;
				if (Dir > 0.0f)
				{
					targetX += 1;
				}

				//Move along the ceiling.
				myPos.x += Dir * Time.deltaTime * ElveConstants.Instance.WalkSpeed;
				Owner.MyTransform.position = myPos;

				//If we've reached our destination, switch states.
				if ((Dir < 0.0f && myPos.x < targetX) ||
					(Dir > 0.0f && myPos.x == targetX))
				{
					Owner.MyTransform.position = new Vector3((float)targetX - (Dir * 0.01f), myPos.y,
															 myPos.z);
					Owner.CurrentState = ToResume;
				}
				break;

			case ElveBehavior.Surfaces.LeftWall:

				Assert.IsTrue((int)Owner.MyTransform.position.x > 0 &&
							  WorldVoxels.IsSolid(WorldVoxels.Instance.Voxels[(int)Owner.MyTransform.position.x - 1,
																			  (int)Owner.MyTransform.position.y]));

				//Animate.
				Owner.MyAnimator.AnimState = ElveAnimStates.ClimbingWall;
				Owner.MyTransform.localScale = new Vector3(-1.0f, Dir, Owner.MyTransform.localScale.z);

				//Figure out the target position to walk to.
				myPos = Owner.MyTransform.position;
				targetY = (int)myPos.y;
				if (Dir > 0.0f)
				{
					targetY += 1;
				}

				//Climb.
				myPos.y += Dir * Time.deltaTime * ElveConstants.Instance.ClimbSpeed;
				Owner.MyTransform.position = myPos;

				//If we've reached out destination, switch states.
				if ((Dir < 0.0f && myPos.y < targetY) ||
					(Dir > 0.0f && myPos.y == targetY))
				{
					Owner.MyTransform.position = new Vector3(myPos.x, (float)targetY - (Dir * 0.01f),
															 myPos.z);
					Owner.CurrentState = ToResume;
				}
				break;

			case ElveBehavior.Surfaces.RightWall:

				Assert.IsTrue((int)Owner.MyTransform.position.x < WorldVoxels.Instance.Voxels.GetLength(0) - 1 &&
							  WorldVoxels.IsSolid(WorldVoxels.Instance.Voxels[(int)Owner.MyTransform.position.x + 1,
																			  (int)Owner.MyTransform.position.y]));

				//Animate.
				Owner.MyAnimator.AnimState = ElveAnimStates.ClimbingWall;
				Owner.MyTransform.localScale = new Vector3(1.0f, Dir, Owner.MyTransform.localScale.z);

				//Figure out the target position to walk to.
				myPos = Owner.MyTransform.position;
				targetY = (int)myPos.y;
				if (Dir > 0.0f)
				{
					targetY += 1;
				}

				//Climb.
				myPos.y += Dir * Time.deltaTime * ElveConstants.Instance.ClimbSpeed;
				Owner.MyTransform.position = myPos;

				//If we've reached out destination, switch states.
				if ((Dir < 0.0f && myPos.y < targetY) ||
					(Dir > 0.0f && myPos.y == targetY))
				{
					Owner.MyTransform.position = new Vector3(myPos.x, (float)targetY - (Dir * 0.01f),
															 myPos.z);
					Owner.CurrentState = ToResume;
				}
				break;

			default:
				Assert.IsTrue(false);
				break;
		}
	}
}