using System;
using UnityEngine;


/// <summary>
/// Makes this object follow a given one.
/// </summary>
public class FollowObject : MonoBehaviour
{
	/// <summary>
	/// If set to null, this script won't do anything.
	/// </summary>
	public Transform ToFollow = null;

	private Transform tr;


	void Awake()
	{
		tr = transform;
	}
	void Update()
	{
		if (ToFollow != null)
		{
			tr.position = ToFollow.position;
		}
	}
}