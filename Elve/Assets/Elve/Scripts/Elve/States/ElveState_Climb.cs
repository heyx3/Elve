using System;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// Climbs up or down to the next voxel space.
/// Assumes the wall surface is solid and the space being moved into is empty.
/// </summary>
public class ElveState_Climb : ElveState
{
	/// <summary>
	/// -1 for climbing down; 1 for climbing up.
	/// </summary>
	public float Dir = 0.0f;

	/// <summary>
	/// The world-space position of the voxel being moved into.
	/// </summary>
	public Vector2i TargetPos { get; private set; }


	public ElveState_Climb(ElveBehavior owner, float dir)
		: base(owner)
	{
		Assert.IsTrue(dir == 1.0f || dir == -1.0f, "Dir must be +/- 1, but it is " + dir);

		Dir = dir;
	}

	public override void OnStateStarting(ElveState oldState)
	{
		Vector3 myPos = Owner.MyTransform.position;

		TargetPos = ToPosI(myPos) + new Vector2i(0, Mathf.RoundToInt(Dir));


		//See if any surface transitions need to be made before engaging in this state.
		VoxelTypes[,] vxs = WorldVoxels.Instance.Voxels;
		switch (Owner.CurrentSurface)
		{
			case ElveBehavior.Surfaces.LeftWall:
				Assert.IsTrue(TargetPos.x > 0);
				if (!WorldVoxels.IsSolid(vxs[TargetPos.x - 1, TargetPos.y]))
				{
					Assert.IsTrue(TargetPos.x < vxs.GetLength(0) - 1);
					Assert.IsTrue(WorldVoxels.IsSolid(vxs[TargetPos.x + 1, TargetPos.y]));

					Owner.CurrentState = new ElveState_ChangeSurface(Owner, this,
																	 ElveBehavior.Surfaces.LeftWall,
																	 ElveBehavior.Surfaces.RightWall);
				}
				break;
			case ElveBehavior.Surfaces.RightWall:
				Assert.IsTrue(TargetPos.x < vxs.GetLength(0) - 1);
				if (!WorldVoxels.IsSolid(vxs[TargetPos.x + 1, TargetPos.y]))
				{
					Assert.IsTrue(TargetPos.x > 0 &&
								  WorldVoxels.IsSolid(vxs[TargetPos.x - 1, TargetPos.y]));

					Owner.CurrentState = new ElveState_ChangeSurface(Owner, this,
																	 ElveBehavior.Surfaces.RightWall,
																	 ElveBehavior.Surfaces.LeftWall);
				}
				break;
				
			case ElveBehavior.Surfaces.Floor:
			case ElveBehavior.Surfaces.Ceiling:
				if (TargetPos.x > 0 && WorldVoxels.IsSolid(vxs[TargetPos.x - 1, TargetPos.y]))
				{
					ElveState changeSurface = new ElveState_ChangeSurface(Owner, this,
																		  Owner.CurrentSurface,
																		  ElveBehavior.Surfaces.LeftWall);

					//If we need to move to the edge of this voxel first, do it.
					int targetX = (int)myPos.x;
					if (Mathf.Abs((float)targetX - myPos.x) < 0.001f)
					{
						Owner.CurrentState = changeSurface;
					}
					else
					{
						Owner.CurrentState = new ElveState_MoveToEdge(Owner, changeSurface, -1.0f);
					}
				}
				else
				{
					Assert.IsTrue(TargetPos.x < vxs.GetLength(0) - 1);
					Assert.IsTrue(WorldVoxels.IsSolid(vxs[TargetPos.x + 1, TargetPos.y]));

					ElveState changeSurface = new ElveState_ChangeSurface(Owner, this,
																		  Owner.CurrentSurface,
																		  ElveBehavior.Surfaces.RightWall);

					//If we need to move to the edge of this voxel first, do it.
					int targetX = (int)myPos.x + 1;
					if (Mathf.Abs((float)targetX - myPos.x) < 0.001f)
					{
						Owner.CurrentState = changeSurface;
					}
					else
					{
						Owner.CurrentState = new ElveState_MoveToEdge(Owner, changeSurface, 1.0f);
					}
				}
				break;

			default:
				Assert.IsTrue(false, "Unknown state " + Owner.CurrentSurface);
				break;
		}
	}
	public override void Update()
	{
		//Update animation/scale.
		Owner.MyAnimator.AnimState = ElveAnimStates.ClimbingWall;
		float xScale;
		if (Owner.CurrentSurface == ElveBehavior.Surfaces.LeftWall)
		{
			xScale = -1.0f;
		}
		else
		{
			Assert.IsTrue(Owner.CurrentSurface == ElveBehavior.Surfaces.RightWall);
			xScale = 1.0f;
		}
		Owner.MyTransform.localScale = new Vector3(xScale, Dir,
												   Owner.MyTransform.localScale.z);

		//Climb.
		Vector3 pos = Owner.MyTransform.position;
		pos.y += Dir * WorldTime.DeltaTime * ElveConstants.Instance.ClimbSpeed;
		if (Owner.CurrentSurface == ElveBehavior.Surfaces.LeftWall)
		{
			pos.x = Mathf.Floor(pos.x) + 0.00001f;
		}
		else
		{
			pos.x = Mathf.Floor(pos.x) + 0.9999f;
		}
		Owner.MyTransform.position = pos;

		//If we reached the destination voxel, stop.
		if ((int)pos.y == TargetPos.y)
		{
			Success();
		}
	}
}