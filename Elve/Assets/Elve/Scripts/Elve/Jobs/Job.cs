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
	/// Allows this job to create stuff like UI elements to show what kind of job this is.
	/// </summary>
	public abstract void OnEnterQueue();
	/// <summary>
	/// Called when this job leaves the job queue.
	/// Generally happens when it's cancelled or when an Elve first commits to doing it.
	/// Allows this job to destroy whatever UI elements it spawned when entering the queue.
	/// </summary>
	public abstract void OnLeavingQueue();


	/// <summary>
	/// A coroutine that makes the given Elve do this job.
	/// NOTE: Make sure at least one update cycle has passed before calling "StopJob()".
	/// </summary>
	public abstract IEnumerator RunJobCoroutine(ElveBehavior elve);

	/// <summary>
	/// Called when the given voxel is changed. Lets this job react to that information.
	/// </summary>
	public abstract void OnBlockChanged(Vector2i block, VoxelTypes newValue);


	/// <summary>
	/// Stops this job, assuming an Elve was in the middle of doing it.
	/// Optionally leaves it in the queue to be picked up by another Elve.
	/// </summary>
	/// <param name="reason">
	/// The message to display in the UI about this job ending, or "null" if nothing should be displayed.
	/// </param>
	/// <param name="moveJobToQueue">
	/// If true, the job is moved back to the queue to be picked up by another Elve.
	/// If false, the job is cancelled.
	/// </param>
	public virtual void StopJob(bool moveJobToQueue, string reason = null)
	{
		//Remove the job from the Elve doing it.

		int index = JobManager.Instance.Elfs.FindIndex(kvp => kvp.Value == this);
		Assert.IsTrue(index >= 0, "Can't find any Elve doing job " + GetType());

		JobManager.Instance.Elfs[index] = new ElveJobKVP(JobManager.Instance.Elfs[index].Key, null);


		//Remove from the "in progress" job list.

		index = JobManager.Instance.JobsInProgress.FindIndex(kvp => kvp.Key == this);
		Assert.IsTrue(index >= 0, "Can't find job in progress: " + GetType());

		JobManager.Instance.StopCoroutine(JobManager.Instance.JobsInProgress[index].Value);
		JobManager.Instance.JobsInProgress.RemoveAt(index);


		//Possibly move the job back to the queue.
		if (moveJobToQueue)
		{
			JobManager.Instance.AddNewJob(this);
		}

		//TODO: Display the message somewhere.
	}
	/// <summary>
	/// Cancels this job, assuming it was in the queue and nobody was working on it.
	/// </summary>
	/// <param name="reason">
	/// The message to display in the UI about this cancellation, or "null" if nothing should be displayed.
	/// </param>
	public virtual void Cancel(string reason)
	{
		Assert.IsFalse(JobManager.Instance.Elfs.Any(kvp => kvp.Value == this));
		Assert.IsFalse(JobManager.Instance.JobsInProgress.Any(kvp => kvp.Key == this));

		JobManager.Instance.Jobs.Remove(this);
		OnLeavingQueue();

		//TODO: Display the message somewhere.
	}
}