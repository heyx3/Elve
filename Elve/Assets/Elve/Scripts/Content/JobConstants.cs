using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A singleton GameObject script containing all necessary constants about jobs/labor.
/// Note that this object will not be destroyed by scene changes.
/// </summary>
public class JobConstants : Singleton<JobConstants>
{
	public float TimeToPlantSeed = 5.0f;


	protected override void Awake()
	{
		base.Awake();
	}
}