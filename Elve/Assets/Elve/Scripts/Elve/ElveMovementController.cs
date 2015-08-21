using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// Sets the Elve's FSM to path towards a target position.
/// </summary>
[RequireComponent(typeof(ElveBehavior))]
public class ElveMovementController : MonoBehaviour
{
	/// <summary>
	/// The path currently being followed, in *reverse* order (for performance reasons).
	/// As each node in the path is reached, it is removed from the end of the list.
	/// </summary>
	[NonSerialized]
	public List<VoxelNode> Path = new List<VoxelNode>();


	/// <summary>
	/// A function that is called once this component successfully finishes a path.
	/// </summary>
	public delegate void PathCompletedCallback(ElveMovementController elve);
	/// <summary>
	/// Raised when a path is successfully finished.
	/// This event's subscribers are cleared out once it is raised, or when a new path is started.
	/// </summary>
	public event PathCompletedCallback OnPathCompleted;

	/// <summary>
	/// Gets whether there are any nodes left to complete in the path.
	/// </summary>
	public bool IsPathCompleted { get { return Path.Count == 0; } }


	private Transform tr;
	private ElveBehavior fsm;

	private Vector2i currentPos, targetPos;


	void Awake()
	{
		tr = transform;
		fsm = GetComponent<ElveBehavior>();
	}


	/// <summary>
	/// Immediately starts pathing towards the given target position.
	/// Replaces any previous pathing commands.
	/// </summary>
	public void StartPath(Vector2 targetWorldPos)
	{
		StartPath(new Vector2i((int)targetWorldPos.x, (int)targetWorldPos.y));
	}
	/// <summary>
	/// Immediately starts pathing towards the given target position.
	/// Replaces any previous pathing commands.
	/// Returns "false" if no path could be found, or "true" otherwise.
	/// </summary>
	public bool StartPath(Vector2i targetWorldPos)
	{
		OnPathCompleted = null;

		targetPos = targetWorldPos;
		Assert.IsTrue(targetPos.x >= 0 && targetPos.y >= 0 &&
					   targetPos.x < WorldVoxels.Instance.Voxels.GetLength(0) &&
					   targetPos.y < WorldVoxels.Instance.Voxels.GetLength(1),
					  "Invalid target position " + targetPos);
		
		UpdatePos();

		PathFinder<VoxelNode> pather = new PathFinder<VoxelNode>(VoxelGraph.Instance,
																 VoxelEdge.MakeEdge);
		pather.Start = new VoxelNode(currentPos);
		pather.End = new VoxelNode(targetPos);
		bool foundPath = pather.FindPath();
		if (!foundPath)
		{
			Path.Clear();
			return false;
		}

		Path = pather.CurrentPath.ToList();
		Path.Reverse();

		//Start the path.
		Assert.IsTrue(Path[Path.Count - 1].WorldPos == currentPos,
					  "Current pos is " + currentPos + " but first node of path is " +
						Path[Path.Count - 1].WorldPos);
		targetPos = currentPos;
		OnStateFinished(null);

		return true;
	}
	private void UpdatePos()
	{
		Vector3 pos = tr.position;
		currentPos.x = (int)pos.x;
		currentPos.y = (int)pos.y;
	}

	private void OnStateFinished(ElveState oldState)
	{
		UpdatePos();

		Assert.IsTrue(currentPos == targetPos,
					  "Target was " + targetPos + " but actual pos is " + currentPos);

		Path.RemoveAt(Path.Count - 1);

		if (Path.Count > 0)
		{
			targetPos = Path[Path.Count - 1].WorldPos;

			Vector2i delta = targetPos - currentPos;
			Assert.IsTrue(delta.x == -1 || delta.x == 1 || delta.y == -1 || delta.y == 1);

			//Choose the correct state based on movement.
			ElveState state;
			if (delta.y == 0)
			{
				state = new ElveState_Walk(fsm, (float)delta.x);
			}
			else if (delta.x == 0)
			{
				state = new ElveState_Climb(fsm, (float)delta.y);
			}
			else
			{
				state = new ElveState_CrossLedge(fsm, delta);
			}
			state.OnStateSucceeded += OnStateFinished;

			fsm.CurrentState = state;
		}
		else
		{
			if (OnPathCompleted != null)
			{
				//Store the event in a temp variable and wipe out the actual event
				//    so new subscribers can be added for the next path.
				PathCompletedCallback tempEvent = (OnPathCompleted + DummyFunc);
				OnPathCompleted = null;
				tempEvent(this);
			}
		}
	}
	private static void DummyFunc(ElveMovementController e) { }

	/// <summary>
	/// Cancels the path currently being walked.
	/// </summary>
	public void CancelPath()
	{
		Path.Clear();
		fsm.CurrentState = null;
	}
}