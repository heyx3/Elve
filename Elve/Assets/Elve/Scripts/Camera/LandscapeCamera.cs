using UnityEngine;


/// <summary>
/// Behavior for the camera to render the landscape.
/// Must be parented to the game camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class LandscapeCamera : MonoBehaviour
{
	public Transform LandscapeSprite;

	private Camera landCam, gameCam;


	void Awake()
	{
		landCam = GetComponent<Camera>();
		gameCam = transform.parent.GetComponent<Camera>();
	}
	void Start()
	{
		landCam.targetTexture = new RenderTexture(gameCam.pixelWidth, gameCam.pixelHeight, 0);
			
			LandscapeSprite.localScale = new Vector3(gameCam.orthographicSize * 2.0f *
														gameCam.pixelWidth / (float)gameCam.pixelHeight,
												     gameCam.orthographicSize * 2.0f,
													 1.0f);
	}
	void Update()
	{
		landCam.orthographicSize = gameCam.orthographicSize;

		if (landCam.targetTexture.width != gameCam.pixelWidth ||
			landCam.targetTexture.height != gameCam.pixelHeight)
		{
			landCam.targetTexture.Release();
			landCam.targetTexture = new RenderTexture(gameCam.pixelWidth, gameCam.pixelHeight, 0);
			
			LandscapeSprite.localScale = new Vector3(gameCam.orthographicSize * gameCam.pixelWidth / (float)gameCam.pixelHeight,
												     gameCam.orthographicSize,
													 1.0f);
		}
	}
}