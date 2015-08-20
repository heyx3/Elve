using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Lists the different kinds of labors this Elve can perform.
/// </summary>
[RequireComponent(typeof(ElveBehavior))]
public class ElveLabors : MonoBehaviour
{
	public ElveBehavior BehaviorFSM { get; private set; }
	public Transform MyTransform { get { return BehaviorFSM.MyTransform; } }


	private Labors enabledLabors = Labors.All;
	

	void Awake()
	{
		BehaviorFSM = GetComponent<ElveBehavior>();
	}
	void Start()
	{
		JobManager.Instance.Elfs.Add(new KeyValuePair<ElveLabors, Job>(this, null));
	}

	public bool HasLabor(Labors l) { return (int)(enabledLabors & l) > 0; }

	public void EnableLabor(Labors l) { enabledLabors |= l; }
	public void DisableLabor(Labors l) { enabledLabors &= ~l; }
}