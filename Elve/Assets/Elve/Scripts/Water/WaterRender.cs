using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles rendering of a water system.
/// </summary>
[RequireComponent(typeof(WaterBehavior))]
public class WaterRender : MonoBehaviour
{
	public Material WaterMat;
	public Mesh Quad;

	private WaterBehavior water;


	void Awake()
	{
		water = GetComponent<WaterBehavior>();
	}
	void OnRenderObject()
	{
		//Don't render if this camera doesn't see this object's layer.
		if ((Camera.current.cullingMask & (1 << gameObject.layer)) == 0)
		{
			return;
		}

		WaterMat.SetPass(0);

		List<WaterDrop> drops = water.Drops;
		for (int i = 0; i < drops.Count; ++i)
		{
			Matrix4x4 mat = Matrix4x4.TRS(new Vector3(drops[i].Pos.x, drops[i].Pos.y, 0.0f),
										  Quaternion.identity,
										  new Vector3(drops[i].Radius * 2.0f, drops[i].Radius * 2.0f, 1.0f));
			Graphics.DrawMeshNow(Quad, mat);
		}
	}
}