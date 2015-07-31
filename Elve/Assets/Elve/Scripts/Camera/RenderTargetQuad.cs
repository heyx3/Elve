using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(RawImage))]
public class RenderTargetQuad : MonoBehaviour
{
	public Camera Cam;

	private RawImage ri;


	void Awake()
	{
		ri = GetComponent<RawImage>();
	}
	void Update()
	{
		ri.texture = Cam.targetTexture;
	}
}