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
			case VoxelTypes.Leaf:
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

	public Image PauseTimeHighlight, QuarterTimeHighlight, HalfTimeHighlight,
				 NormalTimeHighlight, DoubleTimeHighlight, TripleTimeHighlight,
				 QuadrupleTimeHighlight;
	private void EnableHighlight(Image highlight)
	{
		PauseTimeHighlight.enabled = PauseTimeHighlight == highlight;
		QuarterTimeHighlight.enabled = QuarterTimeHighlight == highlight;
		HalfTimeHighlight.enabled = HalfTimeHighlight == highlight;
		NormalTimeHighlight.enabled = NormalTimeHighlight == highlight;
		DoubleTimeHighlight.enabled = DoubleTimeHighlight == highlight;
		TripleTimeHighlight.enabled = TripleTimeHighlight == highlight;
		QuadrupleTimeHighlight.enabled = QuadrupleTimeHighlight == highlight;
	}
	
	
	public Transform CurrentContextMenu { get; private set; }


	/// <summary>
	/// Used when un-pausing the world time.
	/// </summary>
	private float desiredTimeScale = 1.0f;

	
	void Awake()
	{
		CurrentContextMenu = null;

		Assert.IsNotNull(GameCam);
		Assert.IsNotNull(MouseOverlay);
		Assert.IsNotNull(VoxelContextPopup);
	}
	void Start()
	{
		NormalTime();
	}

	void Update()
	{
		Vector3 worldPos = GameCam.ScreenToWorldPoint(Input.mousePosition);

		MouseOverlay.gameObject.SetActive(!Input.GetMouseButton(1));
		if (CurrentContextMenu == null && !TreeInfoWindow.gameObject.activeSelf)
		{
			MouseOverlay.position = new Vector3((int)worldPos.x + 0.5f,
												(int)worldPos.y + 0.5f,
												worldPos.z);
		}

		if (CurrentContextMenu != null && !CurrentContextMenu.gameObject.activeSelf)
		{
			CurrentContextMenu = null;
		}

		//Handle time scale input.
		if (Input.GetButtonDown("Pause Time"))
		{
			//If time is already paused, unpause it.
			if (WorldTime.TimeScale == 0.0f)
			{
				if (desiredTimeScale == 0.25f)
					QuarterTime();
				else if (desiredTimeScale == 0.5f)
					HalfTime();
				else if (desiredTimeScale == 1.0f)
					NormalTime();
				else if (desiredTimeScale == 2.0f)
					DoubleTime();
				else if (desiredTimeScale == 3.0f)
					TripleTime();
				else
				{
					Assert.AreEqual(desiredTimeScale, 4.0f);
					QuadrupleTime();
				}
			}
			else
			{
				PauseTime();
			}
		}
		else if (Input.GetButtonDown("Quarter Time"))
			QuarterTime();
		else if (Input.GetButtonDown("Half Time"))
			HalfTime();
		else if (Input.GetButtonDown("Normal Time"))
			NormalTime();
		else if (Input.GetButtonDown("Double Time"))
			DoubleTime();
		else if (Input.GetButtonDown("Triple Time"))
			TripleTime();
		else if (Input.GetButtonDown("Quadruple Time"))
			QuadrupleTime();
	}

	/// <summary>
	/// Clears any popups currently visible.
	/// Returns whether any popups were actually visible.
	/// </summary>
	public bool ClearPopups()
	{
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
				}
				//No special items were clicked, so a voxel was clicked.
				else
				{
					CurrentContextMenu = VoxelContextPopup.MyTransform;
					if (VoxelContextPopup.SetUp(worldPosI, new Vector2(mousePos.x, mousePos.y)))
					{
						//Nothing needs to be done at this point if the pop-up fails.
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


	public void PauseTime()
	{
		WorldTime.TimeScale = 0.0f;
		EnableHighlight(PauseTimeHighlight);
	}
	public void QuarterTime()
	{
		WorldTime.TimeScale = 0.25f;
		desiredTimeScale = WorldTime.TimeScale;
		EnableHighlight(QuarterTimeHighlight);
	}
	public void HalfTime()
	{
		WorldTime.TimeScale = 0.5f;
		desiredTimeScale = WorldTime.TimeScale;
		EnableHighlight(HalfTimeHighlight);
	}
	public void NormalTime()
	{
		WorldTime.TimeScale = 1.0f;
		desiredTimeScale = WorldTime.TimeScale;
		EnableHighlight(NormalTimeHighlight);
	}
	public void DoubleTime()
	{
		WorldTime.TimeScale = 2.0f;
		desiredTimeScale = WorldTime.TimeScale;
		EnableHighlight(DoubleTimeHighlight);
	}
	public void TripleTime()
	{
		WorldTime.TimeScale = 3.0f;
		desiredTimeScale = WorldTime.TimeScale;
		EnableHighlight(TripleTimeHighlight);
	}
	public void QuadrupleTime()
	{
		WorldTime.TimeScale = 4.0f;
		desiredTimeScale = WorldTime.TimeScale;
		EnableHighlight(QuadrupleTimeHighlight);
	}
}