using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A single drop of water.
/// </summary>
public struct WaterDrop
{
	public Vector2 Pos, Velocity;
	public float Radius;

	/// <summary>
	/// Stored as an optimization for update code.
	/// The voxel grid coordinates of this drop's position.
	/// </summary>
	public Vector2i PosI;


	public float Mass { get { return WaterConstants.Instance.Mass * Radius * Radius; } }


	public WaterDrop(WaterDrop toCopy)
	{
		Pos = toCopy.Pos;
		PosI = toCopy.PosI;
		Velocity = toCopy.Velocity;
		Radius = toCopy.Radius;
	}
	public WaterDrop(Vector2 pos, Vector2 velocity, float radius)
	{
		Pos = pos;
		PosI = new Vector2i((int)pos.x, (int)pos.y);
		Velocity = velocity;
		Radius = radius;
	}

	/// <summary>
	/// Adds forces to push this drop away from the given one.
	/// </summary>
	/// <param name="toOther">The delta from this position to the other drop's position.</param>
	/// <param name="distSqr">The square of the distance between these drops.</param>
	public void Separate(WaterDrop other, Vector2 toOther, float distSqr,
						 ref Vector2 myForce, ref Vector2 otherForce)
	{
		Vector2 forceToOther = toOther / Mathf.Sqrt(distSqr) * WaterConstants.Instance.SeparationForce;
		myForce -= forceToOther;
		otherForce += forceToOther;
	}
	/// <summary>
	/// Combines this drop with the given one.
	/// Returns the result of combining them.
	/// </summary>
	public WaterDrop Combine(WaterDrop other)
	{
		//Weighted-average of positions, conservation of momentum for velocities,
		//    and use sum of surface area to get the new radius.

		float myMass = Mass,
			  otherMass = other.Mass;

		float newMass = myMass + otherMass;
		float newRadius = Mathf.Sqrt(newMass / WaterConstants.Instance.Mass);

		float myWeight = myMass / newMass,
			  otherWeight = 1.0f - myWeight;
		return new WaterDrop((Pos * myWeight) + (other.Pos * otherWeight),
							 ((Velocity * myWeight) + (other.Velocity * otherWeight)),
							 newRadius);
	}
	/// <summary>
	/// Adds normal forces (and related friction forces) based on any blocks this drop is sliding along.
	/// </summary>
	public void AddNormalForces(ref Vector2 myForce)
	{
		float xMin = Pos.x - Radius,
			  xMax = Pos.x + Radius,
			  yMin = Pos.y - Radius,
			  yMax = Pos.y + Radius;
		if ((int)xMin < PosI.x)
		{
			VoxelTypes block = (PosI.x == 0 ?
									VoxelTypes.HardRock :
									WorldVoxels.Instance.Voxels[PosI.x - 1, PosI.y]);

			if (WorldVoxels.IsSolid(block))
			{
				float dist = (float)PosI.x - xMin;
				float normalForce = Mathf.Pow(dist, WaterConstants.Instance.NormalForceGrowth) *
									WaterConstants.Instance.NormalForce;
				myForce.x += normalForce;
				myForce.y += normalForce * -Velocity.y * WaterConstants.Instance.Friction[(int)block].Friction;
			}
		}
		if ((int)xMax > PosI.x)
		{
			VoxelTypes block = (PosI.x == WorldVoxels.Instance.Voxels.GetLength(0) - 1 ?
									VoxelTypes.HardRock :
									WorldVoxels.Instance.Voxels[PosI.x + 1, PosI.y]);

			if (WorldVoxels.IsSolid(block))
			{
				float dist = xMax - (float)(PosI.x + 1);
				float normalForce = Mathf.Pow(dist, WaterConstants.Instance.NormalForceGrowth) *
									WaterConstants.Instance.NormalForce;
				myForce.x += normalForce;
				myForce.y += normalForce * -Velocity.y * WaterConstants.Instance.Friction[(int)block].Friction;
			}
		}
		if ((int)yMin < PosI.y)
		{
			VoxelTypes block = (PosI.y == 0 ?
									VoxelTypes.HardRock :
									WorldVoxels.Instance.Voxels[PosI.x, PosI.y - 1]);

			if (WorldVoxels.IsSolid(block))
			{
				float dist = (float)PosI.y - yMin;
				float normalForce = Mathf.Pow(dist, WaterConstants.Instance.NormalForceGrowth) *
									WaterConstants.Instance.NormalForce;
				myForce.x += normalForce * -Velocity.x * WaterConstants.Instance.Friction[(int)block].Friction;
				myForce.y += normalForce;
			}
		}
		if ((int)yMax > PosI.y)
		{
			VoxelTypes block = (PosI.y == WorldVoxels.Instance.Voxels.GetLength(1) - 1 ?
									VoxelTypes.HardRock :
									WorldVoxels.Instance.Voxels[PosI.x, PosI.y + 1]);

			if (WorldVoxels.IsSolid(block))
			{
				float dist = yMax - (float)(PosI.y + 1);
				float normalForce = Mathf.Pow(dist, WaterConstants.Instance.NormalForceGrowth) *
									WaterConstants.Instance.NormalForce;
				myForce.x += normalForce * -Velocity.x * WaterConstants.Instance.Friction[(int)block].Friction;
				myForce.y += normalForce;
			}
		}
	}
	/// <summary>
	/// Applies this drop's force to its velocity, then its velocity to its position.
	/// Returns the new drop instead of changing this one.
	/// </summary>
	public WaterDrop Update(Vector2 force)
	{
		WaterDrop next = new WaterDrop(this);

		//Apply the force.
		next.Velocity += force * Time.deltaTime / Mass;
		float sizeSqr = next.Velocity.sqrMagnitude;
		if (sizeSqr > WaterConstants.Instance.MaxSpeed)
		{
			next.Velocity /= Mathf.Sqrt(sizeSqr);
			next.Velocity *= WaterConstants.Instance.MaxSpeed;
		}


		//Try applying the velocity and back up if a wall is hit.

		next.Pos += next.Velocity * Time.deltaTime;
		next.PosI = new Vector2i((int)next.Pos.x, (int)next.Pos.y);

		VoxelTypes[,] vxs = WorldVoxels.Instance.Voxels;
		if (next.PosI.x < 0 || next.PosI.x >= vxs.GetLength(0) ||
			WorldVoxels.IsSolid(vxs[next.PosI.x, PosI.y]))
		{
			next.Pos.x = (next.Velocity.x < 0.0f ? PosI.x : (PosI.x + 1));
			next.Velocity.x = -next.Velocity.x * WaterConstants.Instance.BounceDamp;
			next.PosI.x = PosI.x;
		}
		if (next.PosI.y < 0 || next.PosI.y >= vxs.GetLength(1) ||
			WorldVoxels.IsSolid(vxs[PosI.x, next.PosI.y]))
		{
			next.Pos.y = (next.Velocity.y < 0.0f ? PosI.y : (PosI.y + 1));
			next.Velocity.y = -next.Velocity.y * WaterConstants.Instance.BounceDamp;
			next.PosI.y = PosI.y;
		}

		next.Radius -= WaterConstants.Instance.RadiusShrinkRate * Time.deltaTime;

		return next;
	}
}