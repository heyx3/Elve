using System;
using UnityEngine;


public class WaterEmitter : MonoBehaviour
{
	public Vector2 SpawnAreaSize = new Vector2(1.0f, 1.0f);
	public Vector2 MinVelocity = new Vector2(-3.5f, -3.5f),
				   MaxVelocity = new Vector2(3.5f, 3.5f);
	public float MinRadius = 0.01f,
				 MaxRadius = 0.02f;

	public float SpawnInterval = 0.05f;
	public int SpawnAmount = 1;


	private float elapsed = 0.0f;
	private Transform tr;


	void Awake()
	{
		tr = transform;
	}
	void Update()
	{
		elapsed += WorldTime.DeltaTime;
		
		if (elapsed >= SpawnInterval)
		{
			elapsed -= SpawnInterval;

			Vector3 myPos = tr.position;
			Vector2 myPos2 = new Vector2(myPos.x, myPos.y);

			Vector2 halfSize = SpawnAreaSize * 0.5f;

			WaterController.Instance.BurstDrops(myPos2 - halfSize, myPos2 + halfSize,
												MinVelocity, MaxVelocity,
												MinRadius, MaxRadius,
												SpawnAmount);
		}
	}
	void OnDrawGizmosSelected()
	{
		Vector3 myPos = transform.position;

		Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.25f);
		Gizmos.DrawCube(myPos, new Vector3(SpawnAreaSize.x, SpawnAreaSize.y, 0.001f));

		Gizmos.color = new Color(0.0f, 0.0f, 1.0f, 0.375f);
		Gizmos.DrawSphere(myPos, MinRadius);
		Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.375f);
		Gizmos.DrawSphere(myPos, MaxRadius);
	}
}