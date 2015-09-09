using System;
using UnityEngine;
using UIText = UnityEngine.UI.Text;


/// <summary>
/// Displays the wetness of the current block after a certain amount of time.
/// </summary>
[RequireComponent(typeof(UIText))]
public class WetnessDisplay : MonoBehaviour
{
	public float DisplayWaitTime = 1.0f;

	private Transform tr;
	private UIText txt;

	private Vector2i lastPos;
	private float lastPosTime = 0.0f;


	void Awake()
	{
		tr = transform;
		txt = GetComponent<UIText>();
	}
	void Update()
	{
		Vector2 mPos = UIController.Instance.MouseOverlay.position;
		Vector2i mPosI = new Vector2i((int)mPos.x, (int)mPos.y);
		
		//Count how long the mouse has been hovering at this position.
		if (lastPos == mPosI)
		{
			lastPosTime += Time.deltaTime;
		}
		else
		{
			lastPos = mPosI;
			lastPosTime = 0.0f;
			txt.enabled = false;
		}
		
		//If the mouse has been hovering long enough, show the wetness.
		if (WorldVoxels.Instance.Voxels != null && WorldVoxels.IsValidPos(mPosI) &&
			WorldVoxels.IsSolid(WorldVoxels.GetVoxelAt(mPosI)))
		{
			if (lastPosTime >= DisplayWaitTime)
			{
				if (!txt.enabled)
				{
					txt.enabled = true;
				}

				float wetness = WorldVoxels.Instance.Wetness[lastPos.x, lastPos.y];
				string wetStr = wetness.ToString();
				if (wetness >= 1.0f)
				{
					wetStr = "100% wet";
				}
				else if (wetness <= 0.0f)
				{
					wetStr = "0% wet";
				}
				else
				{
					string baseStr = (wetness * 100.000001f).ToString();
					if (baseStr.Contains("."))
					{
						baseStr = baseStr.Substring(0, 4);
					}
					wetStr = baseStr + "% wet";
				}

				txt.text = wetStr;
				tr.position = UIController.Instance.GameCam.WorldToScreenPoint(new Vector3(mPosI.x + 0.5f,
																						   mPosI.y + 0.5f,
																						   tr.position.z));
			}
		}
		else
		{
			txt.enabled = false;
		}
	}
}