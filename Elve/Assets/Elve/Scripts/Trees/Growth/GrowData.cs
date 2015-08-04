using System;


/// <summary>
/// A base class of information about how a tree grows.
/// </summary>
public class GrowData
{
	/// <summary>
	/// The location of the initial seed that the tree grew into.
	/// </summary>
	public Vector2i SproutPos;
	/// <summary>
	/// The type of material this tree is made from.
	/// </summary>
	public VoxelTypes TreeType;

	/// <summary>
	/// The min/max corners of a bounding box that totally encapsulates this tree.
	/// </summary>
	public Vector2i MinBoundBox, MaxBoundBox;


	public GrowData(Vector2i sproutPos, VoxelTypes treeType,
					Vector2i minBoundBox, Vector2i maxBoundBox)
	{
		SproutPos = sproutPos;
		TreeType = treeType;
		MinBoundBox = minBoundBox;
		MaxBoundBox = maxBoundBox;
	}
}