using System;
using System.Collections.Generic;
using Vector2 = UnityEngine.Vector2;
using Mathf = UnityEngine.Mathf;

using Utils = NoiseAlgoUtils;


/// <summary>
/// Provides 2D noise generation algorithms.
/// Is thread-safe (no global state).
/// No strange artifacts or seams when dipping down into negative seed values.
/// </summary>
public static class NoiseAlgos
{
	/// <summary>
	/// A custom PRNG with a nice balance between randomness and speed.
	/// </summary>
	public struct PRNG
	{
		public int Seed;

		public PRNG(int seed = 12345) { Seed = seed; }


		/// <summary>
		/// Returns a random non-negative integer value.
		/// </summary>
		public int NextInt()
		{
			Seed = (Seed ^ 61) ^ (Seed >> 16);
			Seed += (Seed << 3);
			Seed ^= (Seed >> 4);
			Seed *= 0x27d4eb2d;
			Seed ^= (Seed >> 15);
			return Seed;
		}
		/// <summary>
		/// Returns a random value between 0 and 1.
		/// </summary>
		public float NextFloat()
		{
			const int b = 9999999;
			return (float)(NextInt() % b) / (float)b;
		}
	}


	/// <summary>
	/// Returns a pseudo-random value from 0 to 1 using the given seed.
	/// </summary>
	public static float WhiteNoise(Vector2 seed)
	{
		return new PRNG(Utils.GetHashCode(seed)).NextFloat();
	}


	/// <summary>
	/// Returns a pseudo-random value between 0 and 1 based on the given seed's floored value.
	/// </summary>
	public static float GridNoise(Vector2 seed)
	{
		return WhiteNoise(new Vector2(Mathf.Floor(seed.x), Mathf.Floor(seed.y)));
	}

	/// <summary>
	/// Works like GridNoise(), but allows for interpolation instead of hard jumps between values.
	/// </summary>
	public static float InterpolateNoise(Vector2 seed, Func<Vector2, Vector2> tModifier)
	{
		//Get the integer values behind and in front of the seed values.
		float minX = Mathf.Floor(seed.x),
			  maxX = Mathf.Ceil(seed.x),
			  minY = Mathf.Floor(seed.y),
			  maxY = Mathf.Ceil(seed.y);

		//Get the interpolant (will be linear if nothing is done to modify it).
		Vector2 lerp = tModifier(seed - new Vector2(minX, minY));

		return Mathf.Lerp(Mathf.Lerp(WhiteNoise(new Vector2(minX, minY)),
									 WhiteNoise(new Vector2(maxX, minY)),
									 lerp.x),
						  Mathf.Lerp(WhiteNoise(new Vector2(minX, maxY)),
									 WhiteNoise(new Vector2(maxX, maxY)),
									 lerp.x),
						  lerp.y);
	}
	/// <summary>
	/// Like GridNoise(), but with a linear interpolation between values instead of a hard jump.
	/// </summary>
	public static float LinearNoise(Vector2 seed)
	{
		return InterpolateNoise(seed, v => v);
	}
	/// <summary>
	/// Like GridNoise(), but with a smooth interpolation between values instead of a hard jump.
	/// </summary>
	public static float SmoothNoise(Vector2 seed)
	{
		return InterpolateNoise(seed, v =>
			new Vector2(v.x * v.x * (3.0f - (2.0f * v.x)),
						v.y * v.y * (3.0f - (2.0f * v.y))));
	}
	/// <summary>
	/// Like GridNoise(), but with a very smooth interpolation between values instead of a hard jump.
	/// </summary>
	public static float SmootherNoise(Vector2 seed)
	{
		return InterpolateNoise(seed, v =>
			new Vector2(v.x * v.x * v.x * (10.0f + (v.x * (-15.0f + (v.x * 6.0f)))),
						v.y * v.y * v.y * (10.0f + (v.y * (-15.0f + (v.y * 6.0f))))));
	}


	/// <summary>
	/// Computes Worley/Voroni noise for the given seed.
	/// </summary>
	/// <param name="distFunc">
	/// The function to get the distance between two positions.
	/// </param>
	/// <param name="distsToValue">
	/// Takes in the closest and second-closest distances and outputs a noise value from them.
	/// </param>
	/// <example>
	/// //A standard effect is straight-line distance and using the closest distance value.
	/// float noiseVal = WorleyNoise(seed, Vector2.Distance, (f1, f2) => f1);
	/// //Another nice effect is distance squared and using the average of both distance values.
	/// float noiseVal2 = WorleyNoise(seed,
	///								  (v1, v2) => (v1 - v2).sqrMagnitude,
	///								  (f1, f2) => (f1 + f2) * 0.5f);
	/// </example>
	public static float WorleyNoise(Vector2 seed, Func<Vector2, Vector2, float> distFunc,
								    Func<float, float, float> distsToValue)
	{
		//Get the min corner of each of the 9 grid cells near the seed value.
		Vector2 posCenter = new Vector2(Mathf.Floor(seed.x), Mathf.Floor(seed.y)),
				posMinCorner = posCenter - Vector2.one,
				posMaxCorner = posCenter + Vector2.one,
				pos4 = new Vector2(posCenter.x, posMinCorner.y),
				pos5 = new Vector2(posMaxCorner.x, posMinCorner.y),
				pos6 = new Vector2(posMinCorner.x, posCenter.y),
				pos7 = new Vector2(posMaxCorner.x, posCenter.y),
				pos8 = new Vector2(posMinCorner.x, posMaxCorner.y),
				pos9 = new Vector2(posCenter.x, posMaxCorner.y);

		//Get a random point inside each of these cells
		//    and get the distance from the seed pos to the two closest points.
		float min1 = float.PositiveInfinity,
			  min2 = float.PositiveInfinity;
		Utils.GetWorleyMins(ref min1, ref min2, distFunc(Utils.GetWorleyPos(posCenter)	  + posCenter,	  seed));
		Utils.GetWorleyMins(ref min1, ref min2, distFunc(Utils.GetWorleyPos(posMinCorner) + posMinCorner, seed));
		Utils.GetWorleyMins(ref min1, ref min2, distFunc(Utils.GetWorleyPos(posMaxCorner) + posMaxCorner, seed));
		Utils.GetWorleyMins(ref min1, ref min2, distFunc(Utils.GetWorleyPos(pos4)		  + pos4,		  seed));
		Utils.GetWorleyMins(ref min1, ref min2, distFunc(Utils.GetWorleyPos(pos5)		  + pos5,		  seed));
		Utils.GetWorleyMins(ref min1, ref min2, distFunc(Utils.GetWorleyPos(pos6)		  + pos6,		  seed));
		Utils.GetWorleyMins(ref min1, ref min2, distFunc(Utils.GetWorleyPos(pos7)		  + pos7,		  seed));
		Utils.GetWorleyMins(ref min1, ref min2, distFunc(Utils.GetWorleyPos(pos8)		  + pos8,		  seed));
		Utils.GetWorleyMins(ref min1, ref min2, distFunc(Utils.GetWorleyPos(pos9)		  + pos9,		  seed));

		//Filter these distance values into some noise value.
		return distsToValue(min1, min2);
	}
}





public static class NoiseAlgoUtils
{
	/// <summary>
	/// Allows you to reinterpret_cast from float to int and vice-versa.
	/// Used for hashing Unity's Vector2 structure.
	/// </summary>
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
	public struct Reinterpret
	{
		[System.Runtime.InteropServices.FieldOffset(0)]
		public int Int;
		[System.Runtime.InteropServices.FieldOffset(0)]
		public float Float;

		public Reinterpret(int i) { Float = 0.0f; Int = i; }
		public Reinterpret(float f) { Int = 0; Float = f; }
	}


	/// <summary>
	/// A good psuedo-random hash function for Unity's Vector2 class.
	/// </summary>
	public static int GetHashCode(Vector2 v)
	{
		//Reinterpret the floats as ints and get the hash code for them.
		int x = new Reinterpret(v.x).Int,
			y = new Reinterpret(v.y).Int;
		return new Vector2i(x, y).GetHashCode();
	}
	

	/// <summary>
	/// Gets a random position between {0, 0} and {1, 1} given a seed.
	/// Used for Worley/Voroni noise.
	/// </summary>
	public static Vector2 GetWorleyPos(Vector2 seed)
	{
		Vector2 seed2 = new Vector2(seed.y, -seed.x);
		return new Vector2(NoiseAlgos.WhiteNoise(seed),
						   NoiseAlgos.WhiteNoise(seed2));
	}
	/// <summary>
	/// Updates the closest and second-closest values given a new potential value.
	/// Used for Worley/Voroni noise.
	/// </summary>
	public static void GetWorleyMins(ref float min1, ref float min2, float newVal)
	{
		if (newVal < min1)
		{
			min2 = min1;
			min1 = newVal;
		}
		else if (newVal < min2)
		{
			min2 = newVal;
		}
	}
}