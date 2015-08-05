using System;
using UnityEngine;


/// <summary>
/// Manipulates the camera based on keyboard and mouse input.
/// </summary>
public class KeyboardMouseCameraController : MonoBehaviour
{
	/// <summary>
	/// Whether the player is currently dragging the camera with the mouse.
	/// </summary>
	public bool DraggingCamera { get; private set; }


	public Camera GameCam;
	public UIController UIControl;

	public float DragSpeed = 0.005f,
				 KeyMoveSpeed = 0.2f;
	
	public float MinZoom = 1.0f,
				 MaxZoom = 50.0f;
	public float ZoomScale = 1.5f;

	public KeyCode[] UpKeys = new KeyCode[] { KeyCode.W, KeyCode.UpArrow },
					 DownKeys = new KeyCode[] { KeyCode.S, KeyCode.DownArrow },
					 LeftKeys = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow },
					 RightKeys = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };


	private Vector2 lastMousePos;

	Transform tr;


	void Awake()
	{
		tr = transform;

		DraggingCamera = false;

		UnityEngine.Assertions.Assert.AreNotEqual(null, GameCam);
	}
	void Update()
	{
		Vector2 oldCamPos = tr.position;

		//Update dragging input.
		if (Input.GetMouseButton(1))
		{
			if (DraggingCamera)
			{
				Vector3 mPos = Input.mousePosition;
				Vector2 delta = new Vector2(mPos.x, mPos.y) - lastMousePos;

				tr.position += new Vector3(-delta.x * DragSpeed * GameCam.orthographicSize,
										   -delta.y * DragSpeed * GameCam.orthographicSize, 0.0f);

				lastMousePos = Input.mousePosition;
			}
			else
			{
				lastMousePos = Input.mousePosition;
				DraggingCamera = true;
			}
		}
		else
		{
			DraggingCamera = false;
		}

		//Update mouse wheel input.
		float currentSize = GameCam.orthographicSize;
		GameCam.orthographicSize *= Mathf.Pow(ZoomScale, -Input.GetAxis("Mouse ScrollWheel"));
		GameCam.orthographicSize = Mathf.Clamp(GameCam.orthographicSize, MinZoom, MaxZoom);
		UIControl.OnZoom(currentSize / GameCam.orthographicSize);

		//Update keyboard input.
		foreach (KeyCode upKey in UpKeys)
			if (Input.GetKey(upKey))
				tr.position += new Vector3(0.0f, KeyMoveSpeed, 0.0f);
		foreach (KeyCode downKey in DownKeys)
			if (Input.GetKey(downKey))
				tr.position += new Vector3(0.0f, -KeyMoveSpeed, 0.0f);
		foreach (KeyCode leftKey in LeftKeys)
			if (Input.GetKey(leftKey))
				tr.position += new Vector3(-KeyMoveSpeed, 0.0f, 0.0f);
		foreach (KeyCode rightKey in RightKeys)
			if (Input.GetKey(rightKey))
				tr.position += new Vector3(KeyMoveSpeed, 0.0f, 0.0f);


		UIControl.OnPan((Vector2)tr.position - oldCamPos);
	}
}