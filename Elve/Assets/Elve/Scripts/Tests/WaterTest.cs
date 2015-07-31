using System;
using UnityEngine;


public class WaterTest : MonoBehaviour
{
	public WaterBehavior Water;

	public int NDrops = 30;
	public float DropRadiusMin = 0.01f,
				 DropRadiusMax = 0.02f;
	public Vector2 MinVelocity = new Vector2(-3.0f, 0.0f),
				   MaxVelocity = new Vector2(3.0f, -5.0f);
	public bool ContinuousDrop = false;

	public bool ClearAllWater = false;


	void Update()
	{
		if ((ContinuousDrop && Input.GetMouseButton(1)) ||
			(!ContinuousDrop && Input.GetMouseButtonDown(1)))
		{
			Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector2i posI = new Vector2i((int)mPos.x, (int)mPos.y);

			for (int i = 0; i < NDrops; ++i)
			{
				Water.Drops.Add(new WaterDrop(new Vector2(posI.x + UnityEngine.Random.value,
														  posI.y + UnityEngine.Random.value),
											  new Vector2(Mathf.Lerp(MinVelocity.x, MaxVelocity.x,
																	 UnityEngine.Random.value),
														  Mathf.Lerp(MinVelocity.y, MaxVelocity.y,
																	 UnityEngine.Random.value)),
											  Mathf.Lerp(DropRadiusMin, DropRadiusMax, UnityEngine.Random.value)));
			}
		}

		if (ClearAllWater)
		{
			ClearAllWater = false;
			Water.Drops.Clear();
		}
	}
}