using System.Collections.Generic;
using Vector2 = UnityEngine.Vector2;


/// <summary>
/// Provides the items for a context menu that selects the material of seed to plant.
/// The context is the world position of the voxel to plant in.
/// </summary>
public class ContextMenuInitializer_SeedMaterialChoices : ContextMenuInitializer<Vector2i>
{
	#region Menu items

	private class ContextMenuItem_Wood : IContextMenuItem<Vector2i>
	{
		public string Text { get { return "Wood"; } }

		public bool IsItemAvailable(Vector2i plantPos)
		{
			return true;
		}

		public void OnSelected(Vector2i plantPos, Vector2 menuScreenPos)
		{
			UIController contr = UIController.Instance;
			contr.ShowContextMenu(contr.TreePatternContextPopup,
								  new ChooseTreePatternContext(plantPos, VoxelTypes.Item_WoodSeed),
								  menuScreenPos);
		}
	}
	
	#endregion

	public override List<IContextMenuItem<Vector2i>> GetAllItems()
	{
		return new List<IContextMenuItem<Vector2i>>()
		{
			new ContextMenuItem_Wood(),
		};
	}
}