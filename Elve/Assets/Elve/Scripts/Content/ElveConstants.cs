using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A singleton GameObject script containing all necessary constants about Elves.
/// Note that this object will not be destroyed by scene changes.
/// </summary>
public class ElveConstants : MonoBehaviour
{
	public static ElveConstants Instance = null;


	public float WalkSpeed = 1.0f,
				 ClimbSpeed = 1.0f;

	/// <summary>
	/// The max distance the mouse can be from an Elve and still click on it.
	/// </summary>
	public float ClickDistance = 0.25f;

	
	void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("An instance of 'PathingConstants' already exists!");
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}
}