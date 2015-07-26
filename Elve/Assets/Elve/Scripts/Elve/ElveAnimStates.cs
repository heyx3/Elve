public enum ElveAnimStates
{
	Standing,
	Walking,
	ClimbingWall,
	ClimbingCeiling,

	MountingLedge,
	DroppingToLedge,

	//The following are small transitions between surfaces inside an empty voxel.
	FloorToWall,
	FloorToCeiling,
	WallToFloor,
	WallToWall,
	WallToCeiling,
	CeilingToFloor,
	CeilingToWall,
}