using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// A singleton script that distributes jobs to Elfs.
/// </summary>
public class JobManager : Singleton<JobManager>
{
	/// <summary>
	/// Each elf and the job they're currently performing (or null if they're idle).
	/// </summary>
	public List<KeyValuePair<ElveLabors, Job>> Elfs = new List<KeyValuePair<ElveLabors, Job>>();
	/// <summary>
	/// The jobs that haven't been taken yet.
	/// Jobs are organized by priority (first element is the most important).
	/// Use "AddNewJob" to add a job to this list.
	/// </summary>
	public List<Job> Jobs = new List<Job>();
	/// <summary>
	/// The jobs that *have* been taken by an Elve.
	/// </summary>
	public List<Job> JobsInProgress = new List<Job>();


	/// <summary>
	/// Calculates the number of idle Elfs.
	/// </summary>
	public int IdleElfs { get { return Elfs.Count(kvp => kvp.Value == null); } }


	void Update()
	{
		//Try to give each idle Elve a job.
		for (int i = 0; i < Elfs.Count; ++i)
		{
			if (Elfs[i].Value == null)
			{
				for (int j = 0; j < Jobs.Count; ++j)
				{
					if (Elfs[i].Key.HasLabor(Jobs[j].LaborType))
					{
						Elfs[i] = new KeyValuePair<ElveLabors, Job>(Elfs[i].Key, Jobs[i]);

						JobsInProgress.Add(Jobs[j]);
						Jobs.RemoveAt(j);

						Elfs[i].Value.OnLeavingQueue();

						StartCoroutine(Elfs[i].Value.RunJobCoroutine(Elfs[i].Key.BehaviorFSM));

						break;
					}
				}
			}
		}
	}

	/// <summary>
	/// Adds the given job to the back of the queue.
	/// </summary>
	public void AddNewJob(Job job)
	{
		Jobs.Add(job);
		job.OnEnterQueue();
	}
	/// <summary>
	/// Removes the given job from any queues.
	/// Assumes the job is currently being worked on by an Elve.
	/// </summary>
	public void FinishJob(Job job)
	{
		int index = Elfs.FindIndex(kvp => kvp.Value == job);
		UnityEngine.Assertions.Assert.IsTrue(index >= 0, "Couldn't find active job " + job.GetType());
		Elfs[index] = new KeyValuePair<ElveLabors, Job>(Elfs[index].Key, null);

		JobsInProgress.Remove(job);
	}

	/// <summary>
	/// Lets all jobs know that the given block changed values.
	/// </summary>
	public void OnBlockChanged(Vector2i blockPos, VoxelTypes newVal)
	{
		for (int i = 0; i < Jobs.Count; ++i)
		{
			Jobs[i].OnBlockChanged(blockPos, newVal);
		}
		for (int i = 0; i < JobsInProgress.Count; ++i)
		{
			JobsInProgress[i].OnBlockChanged(blockPos, newVal);
		}
	}
}