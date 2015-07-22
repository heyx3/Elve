using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
public class NoiseAlgoTests : MonoBehaviour
{
	public int SizeX = 256,
			   SizeY = 256;
	public bool ShouldGenerate = false;


	[NonSerialized]
	public Texture2D NoiseTex;

	private MeshRenderer rndr;


	void Start()
	{
		rndr = GetComponent<MeshRenderer>();
		NoiseTex = new Texture2D(SizeX, SizeY, TextureFormat.RGBA32, false);
		NoiseTex.filterMode = FilterMode.Point;

		rndr.material.mainTexture = NoiseTex;
	}
	void Update()
	{
		if (ShouldGenerate)
		{
			ShouldGenerate = false;

			//Resize texture.
			if (SizeX != NoiseTex.width || SizeY != NoiseTex.height)
			{
				NoiseTex.Resize(SizeX, SizeY);
			}

			//Generate colors.
			Color[] cols = new Color[SizeX * SizeY];
			for (int y = 0; y < SizeY; ++y)
			{
				for (int x = 0; x < SizeX; ++x)
				{
					int i = x + (y * SizeX);
					Vector2 seed = new Vector2((float)(x - 512),
											   (float)(y - 512)) * 0.01f;

					float noiseVal = NoiseAlgos.LinearNoise(seed);

					cols[i] = new Color(noiseVal, noiseVal, noiseVal);
				}
			}

			//Set pixels in texture.
			NoiseTex.SetPixels(cols);
			NoiseTex.Apply();
		}
	}
}