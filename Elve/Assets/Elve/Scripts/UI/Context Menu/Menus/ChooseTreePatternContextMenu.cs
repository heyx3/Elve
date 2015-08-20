/// <summary>
/// The "context" needed for the "Choose tree pattern" context menu.
/// </summary>
public struct ChooseTreePatternContext
{
	public Vector2i WorldPos;
	public VoxelTypes SeedType;

	public ChooseTreePatternContext(Vector2i worldPos, VoxelTypes seedType)
	{
		WorldPos = worldPos;
		SeedType = seedType;
	}
}


public class ChooseTreePatternContextMenu : ContextMenu<ChooseTreePatternContext> { }