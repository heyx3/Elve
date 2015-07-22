using System;
using System.Collections.Generic;
using UnityEngine;


public class WorldGenerator : MonoBehaviour
{
	public WorldGenSettings Settings = new WorldGenSettings();
	public GameObject ChunkPrefab = null;

	/// <summary>
	/// Meant for use in the inspector to generate the world at will.
	/// </summary>
	public bool ShouldGenerate = false;


	void Start()
	{
		if (ChunkPrefab == null)
		{
			Debug.LogError("'Chunk Prefab' field isnt set to anything!");
		}
		
		if (Settings.Width % Chunk.Size != 0)
		{
			Debug.LogError("World width of " + Settings.Width +
						   " is not divisible by chunk size " + Chunk.Size);
		}
		if (Settings.Height % Chunk.Size != 0)
		{
			Debug.LogError("World height of " + Settings.Height +
						   " is not divisible by chunk size " + Chunk.Size);
		}
	}
	void Update()
	{
		if (ShouldGenerate)
		{
			ShouldGenerate = false;
			Generate();
		}
	}
	

	public void Generate()
	{
		const float chunkSizeF = (float)Chunk.Size;

		Vector2i nChunks = new Vector2i(Settings.Width / Chunk.Size,
										Settings.Height / Chunk.Size);
		for (int cX = 0; cX < nChunks.x; ++cX)
		{
			float cXF = (float)cX;
			for (int cY = 0; cY < nChunks.y; ++cY)
			{
				float cYF = (float)cY;

				Chunk chnk = GenerateChunk(new Vector2i(cX, cY));

				//Fill the chunk's voxels with randomized values.
				for (int x = 0; x < Chunk.Size; ++x)
				{
					float xF = (float)x;
					float worldX = (cXF * chunkSizeF) + xF;

					for (int y = 0; y < Chunk.Size; ++y)
					{
						float yF = (float)y;
						float worldY = (cYF * chunkSizeF) + yF;

						Vector2 seed = new Vector2(worldX, worldY) * 0.1f;
						float noiseVal = NoiseAlgos.SmoothNoise(seed);

						if (noiseVal < Settings.HardRockThreshold)
						{
							chnk.Grid[x, y] = VoxelTypes.SoftRock;
						}
						else
						{
							chnk.Grid[x, y] = VoxelTypes.HardRock;
						}
					}
				}
			}
		}
	}


	/// <summary>
	/// Generates a Chunk object with the given position on the voxel grid and returns it.
	/// </summary>
	private Chunk GenerateChunk(Vector2i minGridPos)
	{
		GameObject obj = (GameObject)Instantiate(ChunkPrefab);
		obj.name = "Chunk " + minGridPos;
		
		Chunk chnk = obj.GetComponent<Chunk>();
		chnk.MinCorner = minGridPos;
		return chnk;
	}
}