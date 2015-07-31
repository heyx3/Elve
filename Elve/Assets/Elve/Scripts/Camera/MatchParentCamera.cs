using UnityEngine;


/// <summary>
/// Behavior for a child camera of the main cam.
/// Must be parented to the game camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class MatchParentCamera : MonoBehaviour
{
	public int Depth0Or16Or24 = 0;

	private Camera myCam, gameCam;


	void Awake()
	{
		myCam = GetComponent<Camera>();
		gameCam = transform.parent.GetComponent<Camera>();
	}
	void Start()
	{
		myCam.targetTexture = new RenderTexture(gameCam.pixelWidth, gameCam.pixelHeight,
												Depth0Or16Or24);
	}
	void Update()
	{
		myCam.orthographicSize = gameCam.orthographicSize;

		if (myCam.targetTexture.width != gameCam.pixelWidth ||
			myCam.targetTexture.height != gameCam.pixelHeight)
		{
			myCam.targetTexture.Release();
			myCam.targetTexture = new RenderTexture(gameCam.pixelWidth, gameCam.pixelHeight,
													Depth0Or16Or24);
		}
	}
}