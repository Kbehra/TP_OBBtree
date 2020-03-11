using UnityEngine;
using System.Collections.Generic;

public class RayTest : MonoBehaviour
{
	protected Transform intersectionGlyph;

	void Awake()
	{
		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

		sphere.transform.localScale = 0.03f * Vector3.one;

		sphere.GetComponent<Renderer>().material.color = Color.red;

		intersectionGlyph = sphere.transform;
	}

	// Update is called once per frame
	void Update ()
	{
		Vector3 origin = transform.position;

		Vector3 direction = transform.forward;

		Debug.DrawRay(origin, 1000.0f * direction);

		intersectionGlyph.gameObject.SetActive(false);	
		
		Vector3 closest_intersection_pt = origin + 1000.0f * direction;

		OBBTree[] obb_trees = FindObjectsOfType<OBBTree>();

		for(int i = 0; i < obb_trees.Length; i++)
		{
			Vector3 intersection_pt;

			bool intersect = obb_trees[i].IntersectRay(new Ray(origin, direction), out intersection_pt);

			if (intersect)
			{
				if ((intersection_pt - origin).sqrMagnitude < (closest_intersection_pt - origin).sqrMagnitude)
				{
					closest_intersection_pt = intersection_pt;

					intersectionGlyph.gameObject.SetActive(true);
					intersectionGlyph.position = closest_intersection_pt;
				}
			}
		}
	}
}
