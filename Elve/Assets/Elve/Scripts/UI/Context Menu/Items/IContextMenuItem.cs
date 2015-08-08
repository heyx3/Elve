using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// An item on a popup context menu.
/// </summary>
/// <typeparam name="Context">
/// The kind of context that is needed for this context menu item.
/// </typeparam>
public interface IContextMenuItem<Context>
{
	/// <summary>
	/// Gets the text to display in the context menu for this item.
	/// </summary>
	string Text { get; }

	/// <summary>
	/// Gets whether this item is available for a context menu with the given context.
	/// </summary>
	bool IsItemAvailable(Context context);

	/// <summary>
	/// Reacts to this item being selected in a context menu.
	/// </summary>
	void OnSelected(Context context);
}