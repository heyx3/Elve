using System;
using UnityEngine;


public class WaterTest : MonoBehaviour
{
	public WaterBehavior Water;

	public int NDrops = 30;
	public float DropRadiusMin = 0.01f,
				 DropRadiusMax = 0.02f;
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
											  new Vector2(Mathf.Lerp(-1.0f, 1.0f, UnityEngine.Random.value), 0.0f),
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