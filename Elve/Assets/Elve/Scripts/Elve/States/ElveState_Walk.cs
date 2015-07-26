using System;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// Walks left or right to the next voxel space.
/// Assumes the floor is solid and the space being moved into is empty.
/// </summary>
public class ElveState_Walk : ElveState
{
	/// <summary>
	/// -1 for walking left; 1 for walking right.
	/// </summary>
	public float Dir = 0.0f;

	/// <summary>
	/// The world-space position of the voxel being moved into.
	/// </summary>
	public Vector2i TargetPos { get; private set; }


	public ElveState_Walk(ElveBehavior owner, float dir)
		: base(owner)
	{
		Assert.IsTrue(dir == 1.0f || dir == -1.0f, "Dir must be +/- 1, but it is " + dir);

		Dir = dir;
	}


	public override void OnStateStarting(ElveState oldState)
	{
		Vector3 myPos = Owner.MyTransform.position;

		TargetPos = ToPosI(myPos) + new Vector2i(Mathf.RoundToInt(Dir), 0);


		//See if any surface transitions need to be made before engaging in this state.
		VoxelTypes[,] vxs = WorldVoxels.Instance.Voxels;
		switch (Owner.CurrentSurface)
		{
			case ElveBehavior.Surfaces.Floor:
				if (TargetPos.y > 0 && !WorldVoxels.IsSolid(vxs[TargetPos.x, TargetPos.y - 1]))
				{
					Assert.IsTrue(TargetPos.y < vxs.GetLength(1) - 1);
					Assert.IsTrue(WorldVoxels.IsSolid(vxs[TargetPos.x, TargetPos.y + 1]));

					Owner.CurrentState = new ElveState_ChangeSurface(Owner, this,
																	 ElveBehavior.Surfaces.Floor,
																	 ElveBehavior.Surfaces.Ceiling);
				}
				break;

			case ElveBehavior.Surfaces.Ceiling:
				Assert.IsTrue(TargetPos.y < vxs.GetLength(1) - 1);
				if (!WorldVoxels.IsSolid(vxs[TargetPos.x, TargetPos.y + 1]))
				{
					Assert.IsTrue(TargetPos.y > 0);
					Assert.IsTrue(WorldVoxels.IsSolid(vxs[TargetPos.x, TargetPos.y - 1]));
					Owner.CurrentState = new ElveState_ChangeSurface(Owner, this,
																	 ElveBehavior.Surfaces.Ceiling,
																	 ElveBehavior.Surfaces.Floor);
				}
				break;

			case ElveBehavior.Surfaces.LeftWall:
			case ElveBehavior.Surfaces.RightWall:
				if (TargetPos.y == 0 || WorldVoxels.IsSolid(vxs[TargetPos.x, TargetPos.y - 1]))
				{
					ElveState changeSurface = new ElveState_ChangeSurface(Owner, this,
																		  Owner.CurrentSurface,
																		  ElveBehavior.Surfaces.Floor);
					
					//If we need to move to the edge of this voxel first, do it.
					int targetY = (int)myPos.y;
					if (Mathf.Abs((float)targetY - myPos.y) < 0.001f)
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
					Assert.IsTrue(TargetPos.y < vxs.GetLength(1) - 1);
					Assert.IsTrue(WorldVoxels.IsSolid(vxs[TargetPos.x, TargetPos.y + 1]));
					
					ElveState changeSurface = new ElveState_ChangeSurface(Owner, this,
																		  Owner.CurrentSurface,
																		  ElveBehavior.Surfaces.Ceiling);
					
					//If we need to move to the edge of this voxel first, do it.
					int targetY = (int)myPos.y + 1;
					if (Mathf.Abs((float)targetY - myPos.y) < 0.001f)
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
				throw new InvalidOperationException("Unknown surface " + Owner.CurrentSurface);
		}
	}


	public override void Update()
	{
		//Update animation.
		if (Owner.CurrentSurface == ElveBehavior.Surfaces.Floor)
		{
			Owner.MyAnimator.AnimState = ElveAnimStates.Walking;
		}
		else
		{
			Assert.IsTrue(Owner.CurrentSurface == ElveBehavior.Surfaces.Ceiling);
			Owner.MyAnimator.AnimState = ElveAnimStates.ClimbingCeiling;
		}
		
		//Flip the Elve correctly.
		Vector3 scale = Owner.MyTransform.localScale;
		Owner.MyTransform.localScale = new Vector3(Dir, scale.y, scale.z);

		//Walk forwards.
		Owner.MyTransform.position += new Vector3(Dir * Time.deltaTime * ElveConstants.Instance.WalkSpeed,
												  0.0f, 0.0f);

		//If we reached the destination voxel, stop.
		if ((int)Owner.MyTransform.position.x == TargetPos.x)
		{
			Owner.CurrentState = null;
		}
	}
}