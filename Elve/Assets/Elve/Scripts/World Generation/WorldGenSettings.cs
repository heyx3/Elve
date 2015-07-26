using System;
using System.Collections.Generic;


[Serializable]
public class WorldGenSettings
{
	public int Width = 128,
			   Height = 256;


	/// <summary>
	/// The start positions of the different "biomes" based on height:
	///   * Rock: mix between hard and soft rock.
	///   * Dirt: mix between soft rock and dirt.
	///   * Surface: empty space.
	/// 0.0 means the value is at the top of the map.
	/// 1.0 means the value is at the bottom of the map.
	/// </summary>
	public float Biome_Rock = 0.2f,
				 Biome_Dirt = 0.3f,
				 Biome_Surface = 0.4f;
	/// <summary>
	/// The amount that the starting heights of the various biomes can vary based on noise.
	/// </summary>
	public float BiomeVariance = 0.047f;
	/// <summary>
	/// The scale for the noise determining biome height variance.
	/// </summary>
	public float BiomeVarianceScale = 0.1f;

	/// <summary>
	/// In "Rock" biome, these are the threshold values for using hard rock instead of soft rock.
	/// The higher the number, the higher the chance of using hard rock.
	/// The "min" value is used at the bottom of the "Rock" biome. The "max" is used at the top.
	/// </summary>
	public float HardRockChanceMin = 1.0f,
				 HardRockChanceMax = 0.0f;
	/// <summary>
	/// In the "Rock" biome, this is the scale of the noise function
	/// used to decide between hard and soft rock.
	/// </summary>
	public float RockScale = 0.1f;

	/// <summary>
	/// In "Dirt" biome, these are the threshold values for using soft rock instead of dirt.
	/// The higher the number, the higher the chance of using soft rock.
	/// The "min" value is used at the bottom of the "Dirt" biome. The "max" is used at the top.
	/// </summary>
	public float SoftRockChanceMin = 1.0f,
				 SoftRockChanceMax = 0.0f;
	/// <summary>
	/// In the "Dirt" biome, this is the scale of the noise function
	/// used to decide between soft rock and dirt.
	/// </summary>
	public float DirtScale = 0.1f;
}