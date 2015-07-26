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
			case ElveBehavior.Surfaces.Floor:
			case ElveBehavior.Surfaces.Ceiling:

				break;

			case ElveBehavior.Surfaces.LeftWall:

				break;
			case ElveBehavior.Surfaces.RightWall:

				break;
		}
	}
	public override void Update()
	{
		
	}
}