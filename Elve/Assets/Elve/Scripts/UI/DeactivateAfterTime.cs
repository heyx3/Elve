using System;
using UnityEngine;


/// <summary>
/// Deactivates this object after a set amount of time has passed.
/// Re-activating the object restarts the timer.
/// </summary>
public class DeactivateAfterTime : MonoBehaviour
{
	/// <summary>
	/// The amount of time to wait before deactivating.
	/// </summary>
	public float WaitTime = 3.0f;

	[NonSerialized]
	public float ElapsedTime = 0.0f;


	void OnEnable()
	{
		ElapsedTime = 0.0f;
	}
	void Update()
	{
		ElapsedTime += Time.deltaTime;
		if (ElapsedTime >= WaitTime)
		{
			gameObject.SetActive(false);
		}
	}
}