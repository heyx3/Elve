using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
/// <summary>
/// A single square group of blocks.
/// </summary>
public class Chunk : MonoBehaviour
{
	/// <summary>
	/// The length of each side of a chunk.
	/// </summary>
	public const int Size = 8;

	/// <summary>
	/// The world grid coordinate of this chunk's left-bottom corner block.
	/// </summary>
	public Vector2i MinCorner = new Vector2i();

	public VoxelTypes[,] Grid = new VoxelTypes[Size, Size];

	public Material VoxelBlockMat;

	private MeshFilter mf;
	private Mesh VoxelMesh { get { return mf.mesh; } set { mf.mesh = value; } }


	void Start()
	{
		mf = GetComponent<MeshFilter>();

		VoxelMesh = new Mesh();
		VoxelMesh.name = "Chunk " + MinCorner + " Mesh";
		VoxelMesh.MarkDynamic();

		RegenMesh();
	}


	/// <summary>
	/// Used in the generation/rendering of voxel meshes.
	/// Each vertex is a point that expands to a quad in the geometry shader.
	/// </summary>
	private struct VoxelVertex
	{
		public Vector2 Pos, PixelMin;
		public VoxelVertex(Vector2 pos, Vector2 pixelMin) { Pos = pos; PixelMin = pixelMin; }
	}

	/// <summary>
	/// Re-calculates the mesh for this chunk of voxels.
	/// </summary>
	public void RegenMesh()
	{
		Vector2 minCornerF = new Vector2((float)MinCorner.x, (float)MinCorner.y) * (float)Size;

		//Generate vertex data.
		List<VoxelVertex> vertices = new List<VoxelVertex>();
		for (int x = 0; x < Grid.GetLength(0); ++x)
		{
			float xF = (float)x;
			for (int y = 0; y < Grid.GetLength(1); ++y)
			{
				if (Grid[x, y] != VoxelTypes.Empty)
				{
					Vector2 pos = minCornerF + new Vector2(xF, (float)y),
							pixel = VoxelContent.Instance.Data[(int)Grid[x, y]].SubTexturePixelMin;
					vertices.Add(new VoxelVertex(pos, pixel));
				}
			}
		}

		//Insert that data into the mesh arrays.
		//Generate indices because Unity apparently NEEDS them.
		Vector3[] poses = new Vector3[vertices.Count];
		Vector2[] uvs = new Vector2[vertices.Count];
		int[] indices = new int[vertices.Count];
		for (int i = 0; i < vertices.Count; ++i)
		{
			poses[i] = new Vector3(vertices[i].Pos.x, vertices[i].Pos.y, 0.0f);
			uvs[i] = vertices[i].PixelMin;
			indices[i] = i;
		}
		
		VoxelMesh.Clear(true);
		VoxelMesh.vertices = poses;
		VoxelMesh.uv = uvs;
		VoxelMesh.SetIndices(indices, MeshTopology.Points, 0);

		VoxelMesh.bounds = new Bounds(new Vector3((MinCorner.x * (float)Size) + (Size * 0.5f),
												  (MinCorner.y * (float)Size) + (Size * 0.5f),
												  0.0f),
									  new Vector3(Size, Size, 1.0f));
	}
}