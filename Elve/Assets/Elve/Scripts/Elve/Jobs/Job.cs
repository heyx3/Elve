using System.Collections;
using System.Linq;
using UnityEngine;
using ElveJobKVP = System.Collections.Generic.KeyValuePair<ElveLabors, Job>;
using Assert = UnityEngine.Assertions.Assert;


public abstract class Job
{
	//TODO: An abstract method that reacts to this job being clicked on.


	public Labors LaborType { get; private set; }

	public string InQueueDescription { get; private set; }
	public string InProgressDescription { get; private set; }


	public Job(Labors laborType, string inQueueDescription, string inProgressDescription)
	{
		LaborType = laborType;
		InQueueDescription = inQueueDescription;
		InProgressDescription = inProgressDescription;
	}


	/// <summary>
	/// Called when this job enters the job queue.
	/// Generally happens when first created or if an Elve abandons it for some reason.
	/// Allows this job to create stuff like UI elements for visibility.
	/// </summary>
	public abstract void OnEnterQueue();
	/// <summary>
	/// Called when this job leaves the job queue.
	/// Generally happens when it's cancelled or when an Elve commits to doing it.
	/// Allows this job to destroy whatever UI elements it spawned when entering the queue.
	/// </summary>
	public abstract void OnLeavingQueue();


	/// <summary>
	/// A coroutine that makes the given Elve do this job.
	/// </summary>
	public abstract IEnumerator RunJobCoroutine(ElveBehavior elve);

	/// <summary>
	/// Called when the given block is changed. Lets this job react to that information.
	/// </summary>
	public abstract void OnBlockChanged(Vector2i block, VoxelTypes newValue);
	/// <summary>
	/// Called when this job gets cancelled somehow.
	/// </summary>
	public abstract void OnCancelled();


	/// <summary>
	/// Makes this job fail: pushes it back in the queue and displays the proper UI response.
	/// Pass "null" to not show a message.
	/// </summary>
	public virtual void Fail(string reason)
	{
		//Remove the job from the Elve doing it.

		int index = JobManager.Instance.Elfs.FindIndex(kvp => kvp.Value == this);
		Assert.IsTrue(index >= 0, "Can't find job " + GetType() + " in 'Elfs' list!");

		JobManager.Instance.Elfs[index] = new ElveJobKVP(JobManager.Instance.Elfs[index].Key, null);


		JobManager.Instance.JobsInProgress.Remove(this);
		JobManager.Instance.AddNewJob(this);

		//TODO: Display the failure somewhere.
	}
}