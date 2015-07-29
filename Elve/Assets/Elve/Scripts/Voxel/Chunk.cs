using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A single square group of blocks.
/// </summary>
public class Chunk
{
	/// <summary>
	/// The length of each side of a chunk.
	/// </summary>
	public const int Size = 8;

	/// <summary>
	/// The world grid coordinate of this chunk's left-bottom corner block.
	/// </summary>
	public Vector2i MinCorner { get; private set; }

	public Mesh VoxelMesh;


	public Chunk(Vector2i minCorner)
	{
		MinCorner = minCorner;

		VoxelMesh = new Mesh();
		VoxelMesh.name = "Chunk " + MinCorner;
		VoxelMesh.MarkDynamic();

		RegenMesh();

		VoxelMesh.bounds = new Bounds(new Vector3((MinCorner.x * Size) + (Size * 0.5f),
												  (MinCorner.y * Size) + (Size * 0.5f),
												  0.0f),
									  new Vector3(Size, Size, 1.0f));
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
		VoxelTypes[,] vxs = WorldVoxels.Instance.Voxels;
		List<VoxelVertex> vertices = new List<VoxelVertex>();
		for (int y = MinCorner.y * Size; y < (MinCorner.y + 1) * Size; ++y)
		{
			UnityEngine.Assertions.Assert.IsTrue(y >= 0 && y < vxs.GetLength(1));

			float yF = (float)y;
			for (int x = MinCorner.x * Size; x < (MinCorner.x + 1) * Size; ++x)
			{
				UnityEngine.Assertions.Assert.IsTrue(x >= 0 && x < vxs.GetLength(0));

				if (vxs[x, y] != VoxelTypes.Empty)
				{
					Vector2 pos = new Vector2((float)x, yF),
							pixel = VoxelContent.Instance.Data[(int)vxs[x, y]].SubTexturePixelMin;
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
	}
}