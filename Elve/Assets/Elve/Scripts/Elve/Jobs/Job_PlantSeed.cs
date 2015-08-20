using System.Collections;
using System.Linq;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


public class Job_PlantSeed : Job
{
	public Vector2i PlantPos;
	public VoxelTypes SeedType;
	public IGrowPattern GrowPattern;

	/// <summary>
	/// The Elve currently attending to this job.
	/// </summary>
	private ElveMovementController elveMovement = null;
	/// <summary>
	/// The UI item that appears in the world.
	/// </summary>
	private Transform uiWidget = null;


	public Job_PlantSeed(Vector2i plantPos, VoxelTypes seedType, IGrowPattern growPattern)
		: base(Labors.Farm, "Plant Seed", "Planting seed")
	{
		PlantPos = plantPos;
		SeedType = seedType;
		GrowPattern = growPattern;
	}


	public override void OnEnterQueue()
	{
		switch (SeedType)
		{
			case VoxelTypes.Item_WoodSeed:
				uiWidget = UIPrefabs.Create_JobPlantWoodSeed(PlantPos);
				break;

			default:
				Assert.IsTrue(false, "Unknown seed type " + SeedType);
				break;
		}
	}
	public override void OnLeavingQueue()
	{
		if (uiWidget != null)
		{
			GameObject.Destroy(uiWidget.gameObject);
		}
	}

	public override IEnumerator RunJobCoroutine(ElveBehavior elve)
	{
		elveMovement = elve.GetComponent<ElveMovementController>();

		//Move to the target.
		if (!elveMovement.StartPath(PlantPos))
		{
			Fail("Path is blocked");
			yield break;
		}
		while (!elveMovement.IsPathCompleted)
			yield return null;

		//Plant the seed.
		Assert.IsNull(elve.CurrentState);
		elve.CurrentState = new ElveState_UseMagic(elve, JobConstants.Instance.TimeToPlantSeed,
												   ElveBehavior.Surfaces.Floor);
		while (elve.CurrentState != null)
			yield return null;

		WorldVoxels.Instance.SetVoxelAt(PlantPos, SeedType);
		SeedManager.Instance.Seeds.Add(new SeedManager.SeedData(PlantPos, GrowPattern));

		elveMovement = null;
		JobManager.Instance.FinishJob(this);
	}

	public override void OnBlockChanged(Vector2i block, VoxelTypes newValue)
	{
		System.Func<VoxelNode, bool> isAffected = (n) =>
		{
			return Mathf.Abs(block.x - n.WorldPos.x) <= 1 &&
				   Mathf.Abs(block.y - n.WorldPos.y) <= 1;
		};

		if (elveMovement != null && elveMovement.Path.Any(isAffected))
		{
			if (!elveMovement.StartPath(PlantPos))
			{
				Fail("Path is blocked");
			}
		}
	}
	public override void OnCancelled()
	{
		ElveBehavior elve = elveMovement.GetComponent<ElveBehavior>();

		//The Elve is either pathing or using magic to make the seed.
		if (elve.CurrentState is ElveState_UseMagic)
		{
			elve.CurrentState = null;
		}
		else
		{
			elveMovement.Path.Clear();
		}
	}

	public override void Fail(string reason)
	{
		base.Fail(reason);
		
		//Stop whichever Elve is working on this job.
		if (elveMovement != null)
		{
			ElveBehavior elve = elveMovement.GetComponent<ElveBehavior>();
			if (elve.CurrentState is ElveState_UseMagic)
			{
				elve.CurrentState = null;
			}
			else
			{
				elveMovement.Path.Clear();
			}

			elveMovement = null;
		}
	}
}