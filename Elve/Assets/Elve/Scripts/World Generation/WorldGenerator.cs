using System;
using System.Collections.Generic;
using UnityEngine;


public class WorldGenerator : MonoBehaviour
{
	public WorldGenSettings Settings = new WorldGenSettings();

	/// <summary>
	/// Meant for use in the inspector to generate the world at will.
	/// </summary>
	public bool ShouldGenerate = false;


	void Start()
	{
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
		Vector2 worldSize = new Vector2((float)Settings.Width, (float)Settings.Height);

		WorldVoxels.Instance.Voxels = new VoxelTypes[Settings.Width, Settings.Height];

		Vector2 posF = new Vector2();
		for (int y = 0; y < Settings.Height; ++y)
		{
			posF.y = (float)y;
			for (int x = 0; x < Settings.Width; ++x)
			{
				posF.x = (float)x;

				WorldVoxels.Instance.Voxels[x, y] = GetVoxel(posF, worldSize);
			}
		}

		WorldVoxels.Instance.GenerateChunksAndConnections();
	}
	/// <summary>
	/// Gets the type of voxel for the given block.
	/// </summary>
	/// <param name="localPos">The position of this block in the chunk's grid.</param>
	/// <param name="worldPos">The position of this block in world space.</param>
	/// <param name="worldSize">The number of blocks in the world along each axis.</param>
	/// <returns>The type of block to place at the given position.</returns>
	private VoxelTypes GetVoxel(Vector2 worldPos, Vector2 worldSize)
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
}