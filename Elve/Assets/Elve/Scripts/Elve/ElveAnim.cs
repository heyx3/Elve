using UnityEngine;


/// <summary>
/// Manages a single Elve animation object.
/// </summary>
[RequireComponent(typeof(Animator))]
public class ElveAnim : MonoBehaviour
{
	private Animator an;


	void Awake()
	{
		an = GetComponent<Animator>();
	}
	void Update()
	{
		an.speed = WorldTime.TimeScale;
	}
}