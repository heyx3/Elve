using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// The main interpreter of UI input on the game world.
/// Is a singleton for easy access.
/// </summary>
public class UIController : MonoBehaviour
{
	public static UIController Instance { get; private set; }


	/// <summary>
	/// Gets a nice, short name for the given type of block.
	/// </summary>
	public static string BlockToString(VoxelTypes t)
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
	/// <summary>
	/// Gets a nice, short name for the given type of tree.
	/// </summary>
	public static string TreeTypeToString(VoxelTypes tree)
	{
		switch (tree)
		{
			case VoxelTypes.Tree_Wood:
				return "Wood";

			default:
				Assert.IsTrue(false, "Unknown tree type " + tree);
				return "UNKNOWN type " + tree.ToString();
		}
	}


	public Camera GameCam;
	public Transform MouseOverlay;

	public TreeWindow TreeInfoWindow;
	public VoxelContextMenu VoxelContextPopup;


	public Transform CurrentContextMenu { get; private set; }

	
	void Awake()
	{
		CurrentContextMenu = null;

		Assert.IsNotNull(GameCam);
		Assert.IsNotNull(MouseOverlay);
		Assert.IsNotNull(VoxelContextPopup);
	}

	void Update()
	{
		Vector3 worldPos = GameCam.ScreenToWorldPoint(Input.mousePosition);
		MouseOverlay.position = new Vector3((int)worldPos.x + 0.5f,
											(int)worldPos.y + 0.5f,
											worldPos.z);

		if (CurrentContextMenu != null && !CurrentContextMenu.gameObject.activeSelf)
		{
			CurrentContextMenu = null;
		}
	}

	/// <summary>
	/// Clears any popups currently visible.
	/// Returns whether any popups were actually visible.
	/// </summary>
	public bool ClearPopups()
	{
		MouseOverlay.gameObject.SetActive(true);

		if (CurrentContextMenu != null)
		{
			CurrentContextMenu.gameObject.SetActive(false);
			CurrentContextMenu = null;
			return true;
		}
		else if (TreeInfoWindow.gameObject.activeSelf)
		{
			TreeInfoWindow.gameObject.SetActive(false);
			return true;
		}

		return false;
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

			//If something is already open, close it.
			//Otherwise, open some kind of popup menu based on what was clicked.
			if (!ClearPopups())
			{
				//Was a tree clicked?
				if (WorldVoxels.IsTree(clickedOn))
				{
					Tree t = WorldTrees.Instance.GetTreeAt(worldPosI, false);
					Assert.IsNotNull(t, "Can't find tree that was clicked on!");

					TreeInfoWindow.gameObject.SetActive(true);
					TreeInfoWindow.SetUpForTree(t);
					
					MouseOverlay.gameObject.SetActive(false);
				}
				//No special items were clicked, so a voxel was clicked.
				else
				{
					CurrentContextMenu = VoxelContextPopup.MyTransform;
					if (VoxelContextPopup.SetUp(worldPosI, new Vector2(mousePos.x, mousePos.y)))
					{
						MouseOverlay.gameObject.SetActive(false);
					}
				}
			}
		}
	}
	public void OnPan(Vector2 amount)
	{
		if (amount == Vector2.zero)
		{
			return;
		}

		ClearPopups();
	}
	public void OnZoom(float amount)
	{
		if (amount == 1.0f)
		{
			return;
		}

		ClearPopups();
	}
}