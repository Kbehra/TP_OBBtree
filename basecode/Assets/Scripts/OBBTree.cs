using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class OBBTree : MonoBehaviour
{
	// Root of the OBBTree. Used as a starting point when parsing the tree
	protected OBBTreeNode root;
	
	// Store the recursivity level of OBB to be drawn
	// Use -1 to disable OBB drawing
	[Range(-1, 100)]
	[SerializeField]
	protected int levelToBeDrawn = -1;
	
	void Update()
	{
		if(levelToBeDrawn > -1)
		{
			Draw(levelToBeDrawn);
		}
	}
	
	public bool IntersectRay(Ray ray, out Vector3 closest_intersection_pt)
	{		
		closest_intersection_pt = new Vector3();

		return false;
	}
	
	// Draw an OBB
	protected void DrawOBB(OBB obb, Color col)
	{
		Vector3 a = new Vector3(obb.bounds.min.x, obb.bounds.min.y, obb.bounds.min.z);
		Vector3 b = new Vector3(obb.bounds.min.x, obb.bounds.min.y, obb.bounds.max.z);
		Vector3 c = new Vector3(obb.bounds.min.x, obb.bounds.max.y, obb.bounds.min.z);
		Vector3 d = new Vector3(obb.bounds.min.x, obb.bounds.max.y, obb.bounds.max.z);
		Vector3 e = new Vector3(obb.bounds.max.x, obb.bounds.min.y, obb.bounds.min.z);
		Vector3 f = new Vector3(obb.bounds.max.x, obb.bounds.min.y, obb.bounds.max.z);
		Vector3 g = new Vector3(obb.bounds.max.x, obb.bounds.max.y, obb.bounds.min.z);
		Vector3 h = new Vector3(obb.bounds.max.x, obb.bounds.max.y, obb.bounds.max.z);

		a = transform.TransformPoint(obb.orientation * a);
		b = transform.TransformPoint(obb.orientation * b);
		c = transform.TransformPoint(obb.orientation * c);
		d = transform.TransformPoint(obb.orientation * d);
		e = transform.TransformPoint(obb.orientation * e);
		f = transform.TransformPoint(obb.orientation * f);
		g = transform.TransformPoint(obb.orientation * g);
		h = transform.TransformPoint(obb.orientation * h);

		Debug.DrawLine(a, b, col);
		Debug.DrawLine(b, d, col);
		Debug.DrawLine(d, c, col);
		Debug.DrawLine(c, a, col);
		Debug.DrawLine(e, f, col);
		Debug.DrawLine(f, h, col);
		Debug.DrawLine(h, g, col);
		Debug.DrawLine(g, e, col);
		Debug.DrawLine(a, e, col);
		Debug.DrawLine(b, f, col);
		Debug.DrawLine(d, h, col);
		Debug.DrawLine(c, g, col);
	}

	// Draw an OBB only if bound to a node located at the level_to_be_drawn recursivity level
	protected void Draw(OBBTreeNode node, int level_to_be_drawn, int current_level)
	{
		if (node != null)
		{
			if (current_level == level_to_be_drawn)
			{
				DrawOBB(node.obb, Color.green);
			}
			else
			{
				if (current_level == level_to_be_drawn - 1)
				{
					DrawOBB(node.obb, Color.gray);
				}

				current_level++;

				foreach (OBBTreeNode child in node.childrenNodes)
				{
					if (child != null)
					{
						Draw(child, level_to_be_drawn, current_level);
					}
				}
			}
		}
	}
	
	protected void Draw(int level_to_be_drawn)
	{
		Draw(root, level_to_be_drawn, 0);
	}
}
