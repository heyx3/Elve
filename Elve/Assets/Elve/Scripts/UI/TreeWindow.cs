using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assert = UnityEngine.Assertions.Assert;


public class TreeWindow : MonoBehaviour
{
	public Text WindowTitle;

	private Tree tree;


	/// <summary>
	/// Sets up this UI window for the given tree.
	/// </summary>
	public void SetUpForTree(Tree tree)
	{
		WindowTitle.text = tree.GrowPattern.TreeType + " Tree";
	}
}