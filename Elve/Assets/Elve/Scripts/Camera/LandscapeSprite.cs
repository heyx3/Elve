using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(RawImage))]
public class LandscapeSprite : MonoBehaviour
{
	public Camera LandscapeCam;

	private RawImage ri;


	void Awake()
	{
		ri = GetComponent<RawImage>();
	}
	void Update()
	{
		ri.texture = LandscapeCam.targetTexture;
	}
}