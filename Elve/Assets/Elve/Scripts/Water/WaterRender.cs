using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles rendering of the water system.
/// </summary>
[RequireComponent(typeof(WaterBehavior))]
public class WaterRender : MonoBehaviour
{
	public Material FinalWaterMat;

	private WaterBehavior water;
	private uint lastFixedUpdate;

	private Mesh waterMesh;


	void Awake()
	{
		water = GetComponent<WaterBehavior>();
		lastFixedUpdate = 0;

		waterMesh = new Mesh();
		waterMesh.name = "Water";
		waterMesh.MarkDynamic();
	}
	void OnRenderObject()
	{
		//If the water has updated since the last render, upload the new water drops.
		if (lastFixedUpdate != water.NFixedUpdates)
		{
			List<WaterDrop> drops = water.Drops;

			//Generate the vertices/indices.
			Vector3[] posAndRadius = new Vector3[drops.Count];
			int[] indices = new int[drops.Count];
			for (int i = 0; i < drops.Count; ++i)
			{
				posAndRadius[i] = new Vector3(drops[i].Pos.x, drops[i].Pos.y, drops[i].Radius);
				indices[i] = i;
			}

			//Update the mesh.
			waterMesh.Clear(true);
			waterMesh.vertices = posAndRadius;
			waterMesh.SetIndices(indices, MeshTopology.Points, 0);

			lastFixedUpdate = water.NFixedUpdates;
		}


		//Don't render if this camera doesn't see this object's layer.
		if ((Camera.current.cullingMask & (1 << gameObject.layer)) == 0)
		{
			return;
		}

		//Render the water mesh.
		FinalWaterMat.SetPass(0);
		Graphics.DrawMeshNow(waterMesh, Matrix4x4.identity);
	}
}