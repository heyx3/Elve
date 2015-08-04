using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A singleton GameObject script containing all necessary data about voxel content.
/// Note that this object will not be destroyed by scene changes.
/// </summary>
public class VoxelContent : MonoBehaviour
{
	public static VoxelContent Instance = null;


	[Serializable]
	/// <summary>
	/// Per-voxel data.
	/// </summary>
	public class VoxelData
	{
		public VoxelTypes Type;
		public Vector2 SubTexturePixelMin;

		public VoxelData(VoxelTypes type, Vector2 subTexturePixelMin)
		{
			Type = type;
			SubTexturePixelMin = subTexturePixelMin;
		}
	}


	public VoxelData[] Data = new VoxelData[(int)VoxelTypes.Empty]
	{
		new VoxelData(VoxelTypes.SoftRock, new Vector2(0.0f, 0.0f)),
		new VoxelData(VoxelTypes.HardRock, new Vector2(35.0f, 0.0f)),
		new VoxelData(VoxelTypes.Dirt, new Vector2(69.0f, 0.0f)),
		new VoxelData(VoxelTypes.Tree_Wood, new Vector2(103.0f, 1.0f)),
		new VoxelData(VoxelTypes.Tree_Wood_Leaf, new Vector2(137.0f, 1.0f)),
	};


	void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("An instance of 'VoxelContent' already exists!");
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);


		//Sanity checks on voxel data.
		if (Data.Length != (int)VoxelTypes.Empty)
		{
			Debug.LogError("There should be exactly " + ((int)VoxelTypes.Empty).ToString() +
						   " types of voxels in 'Data'!");
		}
		for (int i = 0; i < (int)VoxelTypes.Empty; ++i)
		{
			if (Data[i].Type != (VoxelTypes)i)
			{
				Debug.LogError("Entry index " + i + " in 'Data' should be for " + (VoxelTypes)i +
							   ", but it is for " + Data[i].Type);
			}
		}
	}
}