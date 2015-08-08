using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assert = UnityEngine.Assertions.Assert;


public class TreeWindow : MonoBehaviour
{
	public Text WindowTitle, WaterAmount;
	public Toggle AllowGrowthToggle;

	private Tree tree;


	/// <summary>
	/// Sets up this UI window for the given tree.
	/// </summary>
	public void SetUpForTree(Tree tree)
	{
		this.tree = tree;

		WindowTitle.text = UIController.TreeTypeToString(tree.GrowDat.TreeType) + " " +
						   tree.GrowPattern.TreeType + " Tree";
		AllowGrowthToggle.isOn = tree.AllowGrowth;
	}

	/// <summary>
	/// A callback for the "Allow Organic Growth?" checkbox.
	/// </summary>
	public void OnAllowGrowthChanged()
	{
		tree.AllowGrowth = AllowGrowthToggle.isOn;
	}


	void Update()
	{
		WaterAmount.text = "Water: " + System.Math.Round(tree.Water, 3);
	}
}