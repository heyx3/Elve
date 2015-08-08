using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Initializes a context menu by providing the items to display.
/// </summary>
/// <typeparam name="ContextData">
/// The type of context associated with this menu.
/// </typeparam>
public abstract class ContextMenuInitializer<ContextData> : MonoBehaviour
{
	/// <summary>
	/// Gets all potential items the context menu might have to display.
	/// </summary>
	public abstract List<IContextMenuItem<ContextData>> GetAllItems();
}