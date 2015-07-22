using System;
using System.Collections.Generic;


[Serializable]
public class WorldGenSettings
{
	public int Width = 128,
			   Height = 256;

	public float HardRockThreshold = 0.7f;
}