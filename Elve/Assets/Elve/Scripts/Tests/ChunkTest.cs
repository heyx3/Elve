using System;
using System.Collections.Generic;
using UnityEngine;


public class ChunkTest : MonoBehaviour
{
	public Chunk ChunkToModify = null;

	public bool Fill = false,
				SetVaried = false;
	public VoxelTypes Fill_Value = VoxelTypes.Empty;

	
	void Update()
	{
		if (ChunkToModify != null)
		{
			if (Fill)
			{
				Fill = false;

				VoxelTypes[,] block = ChunkToModify.Grid;
				for (int x = 0; x < block.GetLength(0); ++x)
					for (int y = 0; y < block.GetLength(1); ++y)
						block[x, y] = Fill_Value;

				ChunkToModify.RegenMesh();
			}
			else if (SetVaried)
			{
				SetVaried = false;
				
				VoxelTypes[,] block = ChunkToModify.Grid;
				for (int x = 0; x < block.GetLength(0); ++x)
					for (int y = 0; y < block.GetLength(1); ++y)
						block[x, y] = (VoxelTypes)((x + y) % (int)VoxelTypes.Empty);

				ChunkToModify.RegenMesh();
			}
		}
	}
}