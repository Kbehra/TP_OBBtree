using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OBBTreeNode
{
	public OBB obb;
	public OBBTreeNode[] childrenNodes;

    // index of vertex on each triangle of the mesh
    public int[] triangles;

    public OBBTreeNode()
	{
		childrenNodes = new OBBTreeNode[2];

		for (int i = 0; i < 2; i++)
		{
            childrenNodes[i] = null; 
		}
	}
}
