public enum VoxelTypes
{
	SoftRock = 0,
	HardRock,
	Dirt,

	Tree_Wood,
	Leaf,
	TreeBackground,

	//Various items may be placed inside a block.
	Item_WoodSeed,

	/// <summary>
	/// When casted to an int, gives the number of enum values other than "Empty".
	/// </summary>
	Empty,
}