using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The context menu items when selecting a voxel.
/// </summary>
[RequireComponent(typeof(ContextMenu<Vector2i>))]
public class ContextMenuInitializer_Voxel : ContextMenuInitializer<Vector2i>
{
	#region Menu items

	/// <summary>
	/// Plant a seed in empty space.
	/// </summary>
	private struct ContextMenuItem_PlantSeed : IContextMenuItem<Vector2i>
	{
		public string Text { get { return "Plant Seed"; } }

		public bool IsItemAvailable(Vector2i voxelPos)
		{
			VoxelTypes[,] vxs = WorldVoxels.Instance.Voxels;

			//The space must be empty and touching a plantable surface.
			return WorldVoxels.CanPlantIn(vxs[voxelPos.x, voxelPos.y]) &&
				   voxelPos.y > 0 && WorldVoxels.CanPlantOn(vxs[voxelPos.x, voxelPos.y - 1]);
		}
		public void OnSelected(Vector2i voxelPos, Vector2 menuScreenPos)
		{
			UIController contr = UIController.Instance;
			contr.ShowContextMenu(contr.SeedMaterialContextPopup, voxelPos, menuScreenPos);
		}
	}
	
	#endregion

	public override List<IContextMenuItem<Vector2i>> GetAllItems()
	{
		return new List<IContextMenuItem<Vector2i>>()
		{
			new ContextMenuItem_PlantSeed(),
		};
	}
}