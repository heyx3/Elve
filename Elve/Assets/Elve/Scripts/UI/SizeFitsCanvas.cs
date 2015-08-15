using System;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Makes this UI object's RectTransform match the bounds of a given canvas.
/// </summary>
[ExecuteInEditMode]
public class SizeFitsCanvas : MonoBehaviour
{
	public Canvas ToFit;

	private RectTransform toFitTR;
	private RectTransform tr;


	void Awake()
	{
		tr = GetComponent<RectTransform>();
		toFitTR = ToFit.GetComponent<RectTransform>();
	}
	void Update()
	{
		if (toFitTR == null)
			tr.position = ToFit.transform.position;
		else tr.position = toFitTR.position;

		tr.sizeDelta = new Vector2(ToFit.pixelRect.width, ToFit.pixelRect.height);
	}
}