using System;
using UnityEngine;


/// <summary>
/// A singleton script containing all info related to the water system.
/// </summary>
public class WaterConstants : MonoBehaviour
{
	public static WaterConstants Instance;


	[Serializable]
	public class TypeAndFriction
	{
		public VoxelTypes Type;
		public float Friction;
		public TypeAndFriction(VoxelTypes t, float f) { Type = t; Friction = f; }
	}


	/// <summary>
	/// The friction coefficients between water and voxel types.
	/// </summary>
	public TypeAndFriction[] Friction = new TypeAndFriction[]
	{
		new TypeAndFriction(VoxelTypes.SoftRock, 0.1f),
		new TypeAndFriction(VoxelTypes.HardRock, 0.1f),
		new TypeAndFriction(VoxelTypes.Dirt, 0.1f),
		new TypeAndFriction(VoxelTypes.Tree_Wood, 0.1f),
		new TypeAndFriction(VoxelTypes.Tree_Wood_Leaf, 0.001f),
	};

	/// <summary>
	/// An exponent indicating how the normal force grows in strength
	/// as the drop gets closer to the surface.
	/// </summary>
	public float NormalForceGrowth = 4.0f;
	/// <summary>
	/// The scale of the force pushing away from a surface.
	/// </summary>
	public float NormalForce = 2.0f;

	/// <summary>
	/// When a drop hits a surface, it bounces off with this fraction of its original velocity.
	/// </summary>
	public float BounceDamp = 0.5f;

	/// <summary>
	/// The scale of water drops' mass.
	/// </summary>
	public float Mass = 10.0f;

	/// <summary>
	/// The strength/direction of the force of gravity on the drops.
	/// </summary>
	public float Gravity = -9.8f;

	/// <summary>
	/// Any drops moving faster than this will be slowed down.
	/// </summary>
	public float MaxSpeed = 5.0f;

	/// <summary>
	/// When two drops are this fraction of their radius away from each other, they can combine.
	/// </summary>
	public float CombineDistScale = 0.5f;

	/// <summary>
	/// The force pushing large drops apart.
	/// </summary>
	public float SeparationForce = 10.0f;

	/// <summary>
	/// The rate at which a water drop's radius will shrink.
	/// </summary>
	public float RadiusShrinkRate = 0.001f;

	public float MaxRadius = 0.15f;


	void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("An instance of 'WaterConstants' already exists!");
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);


		//Make sure fields are sane.
		int nVals = Enum.GetValues(typeof(VoxelTypes)).Length - 1;
		if (Friction.Length != nVals)
		{
			Debug.LogError("There should be " + nVals + " values in the WaterConstants' 'Friction' array");
		}
		else for (int i = 0; i < nVals; ++i)
		{
			if (Friction[i].Type != (VoxelTypes)i)
			{
				Debug.LogError("Water friction element " + i + " should be for " + (VoxelTypes)i);
			}
		}
	}
}