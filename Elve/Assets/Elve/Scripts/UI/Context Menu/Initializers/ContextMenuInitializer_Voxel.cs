using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The context menu items when selecting a voxel.
/// </summary>
[RequireComponent(typeof(ContextMenu<Vector2i>))]
public class ContextMenuInitializer_Voxel : ContextMenuInitializer<Vector2i>
{
	public override List<IContextMenuItem<Vector2i>> GetAllItems()
	{
		return new List<IContextMenuItem<Vector2i>>()
		{
			new ContextMenuItem_PlantSeed(),
		};
	}
}