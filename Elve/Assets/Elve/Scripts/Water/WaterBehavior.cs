using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Handles water simulation system.
/// </summary>
public class WaterBehavior : MonoBehaviour
{
	private static void ResizeList<T>(List<T> list, int newSize) where T : struct
	{
		if (list.Count > newSize)
		{
			int extras = list.Count - newSize;
			list.RemoveRange(list.Count - extras, extras);
		}
		else if (list.Count < newSize)
		{
			int extras = newSize - list.Count;
			list.AddRange(Enumerable.Repeat(new T(), extras));
		}
	}


	/// <summary>
	/// Double-buffered list so that changes in an update cycle don't affect later drops in that cycle.
	/// </summary>
	private List<WaterDrop>[] dropsBuffers = new List<WaterDrop>[2] { new List<WaterDrop>(), new List<WaterDrop>() };
	/// <summary>
	/// The current drop buffer being used(either 0 or 1).
	/// </summary>
	private int fromIndex = 0;

	public List<WaterDrop> Drops { get { return dropsBuffers[fromIndex]; } }

	/// <summary>
	/// Needs to be tracked for WaterRender to work correctly.
	/// </summary>
	public uint NFixedUpdates { get; private set; }

	void Start()
	{
		NFixedUpdates = 0;
	}
	void FixedUpdate()
	{
		//Don't bother doing anything if the game is paused.
		if (WorldTime.TimeScale == 0.0f)
		{
			return;
		}


		NFixedUpdates += 1;

		List<WaterDrop> from = dropsBuffers[fromIndex],
						to = dropsBuffers[(fromIndex + 1) % 2];

		float timeDelta = Time.fixedDeltaTime * WorldTime.TimeScale;

		//Make sure both lists have the same number of drops.
		ResizeList(to, from.Count);
		ResizeList(forces, from.Count);

		//Initialize stored forces.
		for (int i = 0; i < forces.Count; ++i)
		{
			forces[i] = Vector2.zero;
		}

		//Calculate the force on drops then update them.
		for (int i = 0; i < from.Count; ++i)
		{
			//If the water is invisible, remove it.
			if (from[i].Radius <= 0.0f)
			{
				from.RemoveAt(i);
				to.RemoveAt(i);
				i -= 1;

				break;
			}

			Vector2 force = forces[i];

			//Check for collisions.
			bool collided = false;
			for (int j = i + 1; j < from.Count; ++j)
			{
				float targetDist = from[i].Radius + from[j].Radius,
					  combineDist = targetDist * WaterConstants.Instance.CombineDistScale;
				Vector2 toJ = from[j].Pos - from[i].Pos;
				float distSqr = toJ.sqrMagnitude;
				//If the drops are touching, combine them.
				if (distSqr <= combineDist * combineDist &&
					from[i].Radius < WaterConstants.Instance.MaxRadius &&
					from[j].Radius < WaterConstants.Instance.MaxRadius)
				{
					//Combine the two drops together.
					to[i] = from[i].Combine(from[j]);
					from.RemoveAt(j);
					to.RemoveAt(to.Count - 1);
					collided = true;

					//Run the rest of the update logic for the new drop.
					force.y += WaterConstants.Instance.Gravity;
					to[i].AddNormalForces(ref force);
					to[i] = to[i].Update(force, timeDelta);
					break;
				}
				//Otherwise, push away.
				else if (distSqr <= targetDist * targetDist)
				{
					Vector2 forceJ = forces[j];
					from[i].Separate(from[j], toJ, distSqr, ref force, ref forceJ);
					forces[j] = forceJ;
				}
			}

			if (!collided)
			{
				//Add other forces.
				force.y += WaterConstants.Instance.Gravity;
				from[i].AddNormalForces(ref force);

				to[i] = from[i].Update(force, timeDelta);
				to[i] = to[i].ShrinkRadius(timeDelta);
			}
		}

		fromIndex = (fromIndex + 1) % 2;
	}

	
	/// <summary>
	/// Used in the Update() step.
	/// Is stored as a field instead of a local variable to cut down on allocations.
	/// </summary>
	private List<Vector2> forces = new List<Vector2>();
}