using System.Collections.Generic;
using UnityEngine;

public struct OBB
{
	public enum Axis
	{
		X = 0,
		Y = 1,
		Z = 2,
		Count = 3
	}

	public Quaternion orientation;

	public Bounds bounds;

	public Axis longAxis, medAxis, shortAxis;

	public HashSet<int> containedTriangles;

	/// <summary>
	/// Checks intersection between OBB and ray
	/// </summary>
	/// <param name="ray">Ray (in object space)</param>
	/// <returns>True if ray intersects OBB, false otherwise</returns>
	public bool IntersectRay(Ray ray)
	{
		Ray ray_obb = new Ray();

		ray_obb.origin = Quaternion.Inverse(orientation) * ray.origin;

		ray_obb.direction = (Quaternion.Inverse(orientation) * (ray.origin + ray.direction)) - ray_obb.origin;

		return bounds.IntersectRay(ray_obb);
	}
}
