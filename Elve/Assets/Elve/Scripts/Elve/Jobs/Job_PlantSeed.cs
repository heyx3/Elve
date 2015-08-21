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
		//Spawn a UI widget to show that the seed needs to be planted.
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
		//Destroy the UI widget.
		Assert.IsNotNull(uiWidget);
		GameObject.Destroy(uiWidget.gameObject);
	}

	public override IEnumerator RunJobCoroutine(ElveBehavior elve)
	{
		yield return null;

		elveMovement = elve.GetComponent<ElveMovementController>();

		//Move to the target.
		if (!elveMovement.StartPath(PlantPos))
		{
			StopJob(true, "Path is blocked");
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


		//Stop the job before creating the seed.
		//Otherwise, the changing of that voxel will cause this job to cancel itself.
		StopJob(false);

		WorldVoxels.Instance.SetVoxelAt(PlantPos, SeedType);
		SeedManager.Instance.Seeds.Add(new SeedManager.SeedData(PlantPos, GrowPattern));
	}

	public override void OnBlockChanged(Vector2i block, VoxelTypes newValue)
	{
		//If we can't plant a seed here anymore, the job should be cancelled.
		if (!WorldVoxels.CanPlantOn(WorldVoxels.GetVoxelAt(PlantPos.LessY)) ||
			!WorldVoxels.CanPlantIn(WorldVoxels.GetVoxelAt(PlantPos)))
		{
			if (elveMovement != null)
			{
				StopJob(false, "Can't plant seed anymore");
			}
			else
			{
				Cancel("Can't plant seed anymore");
			}
			return;
		}

		//If there is an Elve doing this job, and his path was affected, recalculate pathing.
		System.Func<VoxelNode, bool> isAffected = (n) =>
		{
			return Mathf.Abs(block.x - n.WorldPos.x) <= 1 &&
				   Mathf.Abs(block.y - n.WorldPos.y) <= 1;
		};
		if (elveMovement != null && elveMovement.Path.Any(isAffected))
		{
			if (!elveMovement.StartPath(PlantPos))
			{
				StopJob(true, "Path became blocked");
				return;
			}
		}
	}

	public override void StopJob(bool moveJobToQueue, string reason = null)
	{
		base.StopJob(moveJobToQueue, reason);

		Assert.IsNotNull(elveMovement);

		//The Elve is either planting the seed or moving to the planting location.
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