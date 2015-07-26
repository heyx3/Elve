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

		Vector2 worldSize = new Vector2((float)Settings.Width, (float)Settings.Height);

		Vector2i nChunks = new Vector2i(Settings.Width / Chunk.Size,
										Settings.Height / Chunk.Size);
		WorldVoxels.Instance.Chunks = new Chunk[nChunks.x, nChunks.y];

		for (int cX = 0; cX < nChunks.x; ++cX)
		{
			float cXF = (float)cX;
			float chunkStartX = (cXF * chunkSizeF);

			for (int cY = 0; cY < nChunks.y; ++cY)
			{
				float cYF = (float)cY;
				float chunkStartY = (cYF * chunkSizeF);

				Chunk chnk = GenerateChunk(new Vector2i(cX, cY));

				//Fill the chunk's voxels with randomized values.
				for (int x = 0; x < Chunk.Size; ++x)
				{
					float xF = (float)x;
					float worldX = chunkStartX + xF;

					for (int y = 0; y < Chunk.Size; ++y)
					{
						float yF = (float)y;
						float worldY = chunkStartY + yF;

						chnk.Grid[x, y] = GetVoxel(new Vector2(xF, yF),
												   new Vector2(worldX, worldY),
												   worldSize);
					}
				}
			}
		}

		WorldVoxels.Instance.GenerateVoxelGrid();
	}
	/// <summary>
	/// Gets the type of voxel for the given block.
	/// </summary>
	/// <param name="localPos">The position of this block in the chunk's grid.</param>
	/// <param name="worldPos">The position of this block in world space.</param>
	/// <param name="worldSize">The number of blocks in the world along each axis.</param>
	/// <returns>The type of block to place at the given position.</returns>
	private VoxelTypes GetVoxel(Vector2 localPos, Vector2 worldPos, Vector2 worldSize)
	{
		//Get which biome we're in based on height.
		float heightLerp = worldPos.y / worldSize.y;
		float variance = NoiseAlgos.LinearNoise(new Vector2(worldPos.x * Settings.BiomeVarianceScale,
														    0.0f));
		heightLerp += Mathf.Lerp(-Settings.BiomeVariance, Settings.BiomeVariance, variance);

		//Bedrock.
		if (heightLerp < Settings.Biome_Rock)
		{
			return VoxelTypes.HardRock;
		}
		//Rock.
		else if (heightLerp < Settings.Biome_Dirt)
		{
			float noise = NoiseAlgos.LinearNoise(worldPos * Settings.RockScale);
			float threshold = Mathf.Lerp(Settings.HardRockChanceMin,
										 Settings.HardRockChanceMax,
										 Mathf.InverseLerp(Settings.Biome_Rock,
														   Settings.Biome_Dirt,
														   heightLerp));
			//Debug.Log("Noise: " + noise + "; Threshold: " + threshold);
			if (noise < threshold)
			{
				return VoxelTypes.HardRock;
			}
			else
			{
				return VoxelTypes.SoftRock;
			}
		}
		//Dirt.
		else if (heightLerp < Settings.Biome_Surface)
		{
			float noise = NoiseAlgos.LinearNoise(worldPos * Settings.DirtScale);
			float threshold = Mathf.Lerp(Settings.HardRockChanceMin,
										 Settings.HardRockChanceMax,
										 Mathf.InverseLerp(Settings.Biome_Dirt,
														   Settings.Biome_Surface,
														   heightLerp));
			if (noise < threshold)
			{
				return VoxelTypes.SoftRock;
			}
			else
			{
				return VoxelTypes.Dirt;
			}
		}
		//Surface.
		else
		{
			return VoxelTypes.Empty;
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
		WorldVoxels.Instance.Chunks[minGridPos.x, minGridPos.y] = chnk;
		return chnk;
	}
}