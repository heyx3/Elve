public enum ElveAnimStates
{
	Standing,

	Walking,
	ClimbingWall,
	ClimbingCeiling,

	//The following are transitions across ledges that also move across voxels.
	MountingLedge,
	DroppingToLedge,
	MountingLedgeUpsideDown,
	DroppingToLedgeUpsideDown,

	//The following are small transitions between surfaces inside a voxel.
	FloorToWall,
	FloorToCeiling,
	WallToFloor,
	WallToWall,
	WallToCeiling,
	CeilingToFloor,
	CeilingToWall,
}