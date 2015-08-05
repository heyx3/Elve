using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// The main interpreter of UI input on the game world.
/// </summary>
public class UIController : MonoBehaviour
{
	/// <summary>
	/// Gets a nice, short name for the given type of voxel.
	/// </summary>
	public static string ToString(VoxelTypes t)
	{
		switch (t)
		{
			case VoxelTypes.Dirt:
				return "Dirt";
			case VoxelTypes.SoftRock:
				return "Soft Rock";
			case VoxelTypes.HardRock:
				return "Hard Rock";
			case VoxelTypes.Empty:
				return "Empty Space";
			case VoxelTypes.Tree_Wood:
				return "Wood";
			case VoxelTypes.Tree_Wood_Leaf:
				return "Leaves";

			default:
				Assert.IsTrue(false, "Unknown voxel type " + t);
				return "UNKNOWN type " + t.ToString();
		}
	}


	public Camera GameCam;

	public TreeWindow TreeInfoWindow;

	
	void Awake()
	{
		Assert.IsNotNull(GameCam);
	}


	public void OnClick()
	{
		//Get the world position that was clicked on.
		Vector3 mousePos = Input.mousePosition;
		Vector3 worldPos = GameCam.ScreenToWorldPoint(mousePos);
		Vector2i worldPosI = new Vector2i((int)worldPos.x, (int)worldPos.y);

		if (worldPosI.x >= 0 && worldPosI.y >= 0 &&
			worldPosI.x < WorldVoxels.Instance.Voxels.GetLength(0) &&
			worldPosI.y < WorldVoxels.Instance.Voxels.GetLength(1))
		{
			VoxelTypes clickedOn = WorldVoxels.Instance.Voxels[worldPosI.x, worldPosI.y];
			switch (clickedOn)
			{
				case VoxelTypes.Dirt:
				case VoxelTypes.SoftRock:
				case VoxelTypes.HardRock:
				case VoxelTypes.Empty:

					//Disable any open windows.
					if (TreeInfoWindow.gameObject.activeSelf)
					{
						TreeInfoWindow.gameObject.SetActive(false);
					}

					break;

				case VoxelTypes.Tree_Wood:
				case VoxelTypes.Tree_Wood_Leaf:

					Tree t = WorldTrees.Instance.GetTreeAt(worldPosI,
														   (clickedOn == VoxelTypes.Tree_Wood_Leaf));
					Assert.IsNotNull(t, "Can't find tree that was clicked on!");

					TreeInfoWindow.gameObject.SetActive(true);
					TreeInfoWindow.SetUpForTree(t);

					break;


				default:
					Assert.IsTrue(false, "Unknown voxel type " + clickedOn);
					break;
			}
		}
	}
	public void OnPan(Vector2 amount)
	{

	}
	public void OnZoom(float amount)
	{

	}
}