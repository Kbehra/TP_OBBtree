using UnityEngine;

namespace Utils
{
	/// <summary>
	/// Geometry utilities for Unity
	/// </summary>
	static public class UnityGeometry
	{
		private const float epsilon = 1E-05F;

		/// <summary>
		/// Compute intersection between a triangle and a segment
		/// </summary>
		/// <param name="v1">Position of triangle 1st vertex</param>
		/// <param name="v2">Position of triangle 2nd vertex</param>
		/// <param name="v3">Position of triangle 3rd vertex</param>
		/// <param name="origin">Segment origin</param>
		/// <param name="extremity">Segment extremity</param>
		/// <param name="intersection_pt">Placeholder for intersection point, if it exists</param>
		/// <returns>True if segment intersects triangle, false otherwise</returns>
		static public bool GetSegmentTriangleIntersection(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 origin, Vector3 extremity, out Vector3 intersection_pt)
		{
			Vector3 n = Vector3.Cross(v2 - v1, v3 - v1);
			n.Normalize();

			if (GetSegmentPlaneIntersection(n, v1, origin, extremity, out intersection_pt))
			{
				if(IsPointInTriangle(v1, v2, v3, intersection_pt))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Compute intersection between a line and a plane
		/// </summary>
		/// <param name="n">Plane normal</param>
		/// <param name="p">Point belonging to the plane</param>
		/// <param name="dir">Line direction</param>
		/// <param name="m">Point belonging to the line</param>
		/// <param name="intersection_pt"></param>
		/// <returns>Placeholder for intersection point, if it exists</returns>
		static public bool GetLinePlaneIntersection(Vector3 n, Vector3 p, Vector3 dir, Vector3 m, out Vector3 intersection_pt)
		{
			intersection_pt = new Vector3();

			dir.Normalize();
			
			float d = -n.x * p.x - n.y * p.y - n.z * p.z;

			float denom = n.x * dir.x + n.y * dir.y + n.z * dir.z;

			if(denom != 0.0f)
			{
				float num = -(n.x * m.x + n.y * m.y + n.z * m.z + d);

				float t = num / denom;

				intersection_pt = m + dir * t;

				return true;
			}

			return false;
		}

		/// <summary>
		/// Compute intersection between a plane and a segment
		/// </summary>
		/// <param name="n">Plane normal</param>
		/// <param name="p">Point belonging to the plane</param>
		/// <param name="origin">Segment origin</param>
		/// <param name="extremity">Segment extremity</param>
		/// <param name="intersection_pt">Placeholder for intersection point, if it exists</param>
		/// <returns>True if segment intersects plane, false otherwise</returns>
		static public bool GetSegmentPlaneIntersection(Vector3 n, Vector3 p, Vector3 origin, Vector3 extremity, out Vector3 intersection_pt)
		{
			intersection_pt = new Vector3();

			if (Vector3.Dot(n, origin - p) * Vector3.Dot(n, extremity - p) >= 0)
			{
				return false;
			}

			// Compute intersection
			return GetLinePlaneIntersection(n, p, extremity - origin, origin, out intersection_pt);
		}

		/// <summary>
		/// Check whether a point lies inside a triangle
		/// (point is supposed to lie in the triangle plane)
		/// </summary>
		/// <param name="v1">Position of triangle 1st vertex</param>
		/// <param name="v2">Position of triangle 2nd vertex</param>
		/// <param name="v3">Position of triangle 3rd vertex</param>
		/// <param name="point_to_test">Position of point to be tested</param>
		/// <returns>True if point lies inside triangle, false otherwise</returns>
		static public bool IsPointInTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 point_to_test)
		{
			Vector3 u = v1 - point_to_test;
			Vector3 v = v2 - point_to_test;
			Vector3 w = v3 - point_to_test;

			float sum = Vector3.Angle(u, v) + Vector3.Angle(v, w) + Vector3.Angle(w, u);

			return Mathf.Abs(sum - 360.0f) < 100.0f * epsilon;	// avoid floating point accuracy issue
		}

		/// <summary>
		/// Compute triangle vs triangle intersection
		/// </summary>
		/// <param name="v1">Position of 1st triangle 1st vertex</param>
		/// <param name="v2">Position of 1st triangle 2nd vertex</param>
		/// <param name="v3">Position of 1st triangle 3rd vertex</param>
		/// <param name="u1">Position of 2nd triangle 1st vertex</param>
		/// <param name="u2">Position of 2nd triangle 2nd vertex</param>
		/// <param name="u3">Position of 2nd triangle 3rd vertex</param>
		/// <returns>true if triangles intersect, false otherwise</returns>
		static public bool GetTriangleTriangleIntersection(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 u1, Vector3 u2, Vector3 u3)
		{
			Vector3 intersect;

			if (ArePlanesCoPlanar(v1, v2, v3, u1, u2, u3))
			{
				// Intersection occurs as soon as two segments intersect.

				// 1st triangle 1st edge vs 2nd triangle 1st edge
				if(ComputeSegmentSegmentIntersection(v1, v2, u1, u2, out intersect))
				{
					return true;
				}

				// 1st triangle 1st edge vs 2nd triangle 2nd edge
				if (ComputeSegmentSegmentIntersection(v1, v2, u2, u3, out intersect))
				{
					return true;
				}

				// 1st triangle 1st edge vs 2nd triangle 3rd edge
				if (ComputeSegmentSegmentIntersection(v1, v2, u1, u3, out intersect))
				{
					return true;
				}

				
				// 1st triangle 2nd edge vs 2nd triangle 1st edge
				if (ComputeSegmentSegmentIntersection(v2, v3, u1, u2, out intersect))
				{
					return true;
				}

				// 1st triangle 2nd edge vs 2nd triangle 2nd edge
				if (ComputeSegmentSegmentIntersection(v2, v3, u2, u3, out intersect))
				{
					return true;
				}

				// 1st triangle 2nd edge vs 2nd triangle 3rd edge
				if (ComputeSegmentSegmentIntersection(v2, v3, u1, u3, out intersect))
				{
					return true;
				}


				// 1st triangle 3rd edge vs 2nd triangle 1st edge
				if (ComputeSegmentSegmentIntersection(v1, v3, u1, u2, out intersect))
				{
					return true;
				}

				// 1st triangle 3rd edge vs 2nd triangle 2nd edge
				if (ComputeSegmentSegmentIntersection(v1, v3, u2, u3, out intersect))
				{
					return true;
				}

				// 1st triangle 3rd edge vs 2nd triangle 3rd edge
				if (ComputeSegmentSegmentIntersection(v1, v3, u1, u3, out intersect))
				{
					return true;
				}


				return false;
			}
			else
			{
				// Intersection occurs as soon as we find one edge intersecting one triangle. No need to test for a second edge.

				// 1st triangle vs 2nd triangle 1st edge
				if(GetSegmentTriangleIntersection(v1, v2, v3, u1, u2, out intersect))
				{
					return true;
				}

				// 1st triangle vs 2nd triangle 2nd edge
				if(GetSegmentTriangleIntersection(v1, v2, v3, u2, u3, out intersect))
				{
					return true;
				}

				// 1st triangle vs 2nd triangle 3rd edge
				if(GetSegmentTriangleIntersection(v1, v2, v3, u1, u3, out intersect))
				{
					return true;
				}


				// 2nd triangle vs 1st triangle 1st edge
				if(GetSegmentTriangleIntersection(u1, u2, u3, v1, v2, out intersect))
				{
					return true;
				}

				// 2nd triangle vs 1st triangle 2nd edge
				if(GetSegmentTriangleIntersection(u1, u2, u3, v2, v3, out intersect))
				{
					return true;
				}

				// 2nd triangle vs 1st triangle 3dr edge
				if(GetSegmentTriangleIntersection(u1, u2, u3, v1, v3, out intersect))
				{
					return true;
				}


				return false;
			}
		}

		/// <summary>
		/// Compute shortest segment between two lines
		/// </summary>
		/// <param name="line1_point1">1st line 1st point</param>
		/// <param name="line1_point2">1st line 2nd point</param>
		/// <param name="line2_point1">2nd line 1st point</param>
		/// <param name="line2_point2">2nd line 2nd point</param>
		/// <param name="result_point1">segment 1st point</param>
		/// <param name="result_point2">segment 2nd point</param>
		/// <returns></returns>
		static public bool GetShortestSegmentBetweenLines(Vector3 line1_point1, Vector3 line1_point2, Vector3 line2_point1, Vector3 line2_point2, out Vector3 result_point1, out Vector3 result_point2)
		{
			// Algorithm is ported from the C algorithm of 
			// Paul Bourke at http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline3d/
			result_point1 = Vector3.zero;
			result_point2 = Vector3.zero;

			Vector3 p1 = line1_point1;
			Vector3 p2 = line1_point2;
			Vector3 p3 = line2_point1;
			Vector3 p4 = line2_point2;
			Vector3 p13 = p1 - p3;
			Vector3 p43 = p4 - p3;

			if (p43.sqrMagnitude < epsilon)
			{
				return false;
			}

			Vector3 p21 = p2 - p1;

			if (p21.sqrMagnitude < epsilon)
			{
				return false;
			}

			float d1343 = p13.x * p43.x + p13.y * p43.y + p13.z * p43.z;
			float d4321 = p43.x * p21.x + p43.y * p21.y + p43.z * p21.z;
			float d1321 = p13.x * p21.x + p13.y * p21.y + p13.z * p21.z;
			float d4343 = p43.x * p43.x + p43.y * p43.y + p43.z * p43.z;
			float d2121 = p21.x * p21.x + p21.y * p21.y + p21.z * p21.z;

			float denom = d2121 * d4343 - d4321 * d4321;

			if (Mathf.Abs(denom) < epsilon)
			{
				return false;
			}

			float numer = d1343 * d4321 - d1321 * d4343;

			float mua = numer / denom;
			float mub = (d1343 + d4321 * (mua)) / d4343;

			result_point1.x = (p1.x + mua * p21.x);
			result_point1.y = (p1.y + mua * p21.y);
			result_point1.z = (p1.z + mua * p21.z);
			result_point2.x = (p3.x + mub * p43.x);
			result_point2.y = (p3.y + mub * p43.y);
			result_point2.z = (p3.z + mub * p43.z);

			return true;
		}

		/// <summary>
		/// Compute intersection between 2 segments
		/// </summary>
		/// <param name="segment1_point1">1st segment 1st point</param>
		/// <param name="segment1_point2">1st segment 2nd point</param>
		/// <param name="segment2_point1">2nd segment 1st point</param>
		/// <param name="segment2_point2">2nd segment 2nd point</param>
		/// <param name="intersection_pt">intersection point</param>
		/// <returns></returns>
		static public bool ComputeSegmentSegmentIntersection(Vector3 segment1_point1, Vector3 segment1_point2, Vector3 segment2_point1, Vector3 segment2_point2, out Vector3 intersection_pt)
		{
			intersection_pt = Vector3.zero;

			if(ComputeLineLineIntersection(segment1_point1, segment1_point2, segment2_point1, segment2_point2, out intersection_pt))
			{
				float s1 = Vector3.Dot(segment1_point1 - intersection_pt, segment1_point2 - intersection_pt);
				float s2 = Vector3.Dot(segment2_point1 - intersection_pt, segment2_point2 - intersection_pt);

				if(s1 < 0.0f && s2 < 0.0f)
				{
					return true;
				}
			}

			return false;
		}

		static public bool ComputeLineLineIntersection(Vector3 line1_point1, Vector3 line1_point2, Vector3 line2_point1, Vector3 line2_point2, out Vector3 intersection_pt)
		{
			Vector3 v1, v2;

			intersection_pt = Vector3.zero;

			if (GetShortestSegmentBetweenLines(line1_point1, line1_point2, line2_point1, line2_point2, out v1, out v2))
			{
				if((v2 - v1).sqrMagnitude < epsilon)
				{
					intersection_pt = v1;

					return true;
				}
			}

			return false;
		}

		static public bool ArePlanesCoPlanar(Vector3 u1, Vector3 u2, Vector3 u3, Vector3 v1, Vector3 v2, Vector3 v3)
		{
			Vector3 n1 = Vector3.Cross(u2 - u1, u3 - u1);
			Vector3 n2 = Vector3.Cross(v2 - v1, v3 - v1);

			return ArePlanesCoPlanar(n1, u1, n2, v1);
		}

		static public bool ArePlanesCoPlanar(Vector3 n1, Vector3 p1, Vector3 n2, Vector3 p2)
		{
			Vector3 cross = Vector3.Cross(n1, n2);

			if (cross.sqrMagnitude < epsilon)
			{
				// normals are colinear, so planes are parallel or coplanar
				
				// p1 and p2 belong to the same plane if p1p2 is orthogonal to either n1 or n2
				// thus both planes are coplanar
				if(Mathf.Abs(Vector3.Dot(p2 - p1, n1)) < epsilon)
				{
					return true;
				} 
			}

			return false;
		}

		/// <summary>
		/// Computes intersection between a line and a box.
		/// </summary>
		/// <param name="box_min">Box minimum corner</param>
		/// <param name="box_max">Box maximum corner</param>
		/// <param name="dir">Line direction</param>
		/// <param name="b">Point belonging to the line</param>
		/// <returns>True if line intersects box, false otherwise</returns>
		static public bool GetLineBoxIntersection(Vector3 box_min, Vector3 box_max, Vector3 dir, Vector3 p)
		{
			// Store 1st intersection of line with either xmin or xmax planes, sorted along line
			Vector3? xmin = null;

			// Store 2nd intersection of line with either xmin or xmax planes, sorted along line
			Vector3? xmax = null;
			
			// If intersections exist
			if(Mathf.Abs(dir.x) > epsilon)
			{
				// Compute intersection of line with xmin plane
				float t1 = (box_min.x - p.x) / dir.x;
				Vector3 x1 = p + t1 * dir;

				// Compute intersection of line with xmax plane
				float t2 = (box_max.x - p.x) / dir.x;
				Vector3 x2 = p + t2 * dir;

				// Sort x1 and x2 along dir
				if (Vector3.Dot(x2 - x1, dir) > 0)
				{
					xmin = x1;
					xmax = x2;
				}
				else
				{
					xmin = x2;
					xmax = x1;
				}
			}
			else
			{
				// Intersection does not exist
				// Is line between planes or outside?
				if(p.x < box_min.x || p.x > box_max.x)
				{
					return false;
				}
			}

			// Store 1st intersection of line with either ymin or ymax planes, sorted along line
			Vector3? ymin = null;

			// Store 2nd intersection of line with either ymin or ymax planes, sorted along line
			Vector3? ymax = null;

			// If intersections exist
			if (Mathf.Abs(dir.y) > epsilon)
			{
				// Compute intersection of line with ymin plane
				float t1 = (box_min.y - p.y) / dir.y;
				Vector3 y1 = p + t1 * dir;

				// Compute intersection of line with ymax plane
				float t2 = (box_max.y - p.y) / dir.y;
				Vector3 y2 = p + t2 * dir;

				// Sort y1 and y2 along dir
				if (Vector3.Dot(y2 - y1, dir) > 0)
				{
					ymin = y1;
					ymax = y2;
				}
				else
				{
					ymin = y2;
					ymax = y1;
				}
			}
			else
			{
				// Intersection does not exist
				// Is line between planes or outside?
				if (p.y < box_min.y || p.y > box_max.y)
				{
					return false;
				}
			}

			// Store 1st intersection of line with either zmin or zmax planes, sorted along line
			Vector3? zmin = null;

			// Store 2nd intersection of line with either zmin or zmax planes, sorted along line
			Vector3? zmax = null;

			// If intersections exist
			if (Mathf.Abs(dir.z) > epsilon)
			{
				// Compute intersection of line with zmin plane
				float t1 = (box_min.z - p.z) / dir.z;
				Vector3 z1 = p + t1 * dir;

				// Compute intersection of line with zmax plane
				float t2 = (box_max.z - p.z) / dir.z;
				Vector3 z2 = p + t2 * dir;

				// Sort z1 and z2 along dir
				if (Vector3.Dot(z2 - z1, dir) > 0)
				{
					zmin = z1;
					zmax = z2;
				}
				else
				{
					zmin = z2;
					zmax = z1;
				}
			}
			else
			{
				// Intersection does not exist
				// Is line between planes or outside?
				if (p.z < box_min.z || p.z > box_max.z)
				{
					return false;
				}
			}


			// Get max of mins
			Vector3? max_of_mins = xmin;

			if(max_of_mins.HasValue)
			{
				if(ymin.HasValue)
				{
					if (Vector3.Dot(ymin.Value - max_of_mins.Value, dir) > 0)
					{
						max_of_mins = ymin;
					}
				}
			}
			else
			{
				max_of_mins = ymin;
			}

			if (max_of_mins.HasValue)
			{
				if (zmin.HasValue)
				{
					if (Vector3.Dot(zmin.Value - max_of_mins.Value, dir) > 0)
					{
						max_of_mins = zmin;
					}
				}
			}
			else
			{
				max_of_mins = zmin;
			}

			// Get min of maxs
			Vector3? min_of_maxs = xmax;

			if (min_of_maxs.HasValue)
			{
				if (ymax.HasValue)
				{
					if (Vector3.Dot(min_of_maxs.Value - ymax.Value, dir) > 0)
					{
						min_of_maxs = ymax;
					}
				}
			}
			else
			{
				min_of_maxs = ymax;
			}

			if (min_of_maxs.HasValue)
			{
				if (zmax.HasValue)
				{
					if (Vector3.Dot(min_of_maxs.Value - zmax.Value, dir) > 0)
					{
						min_of_maxs = zmax;
					}
				}
			}
			else
			{
				min_of_maxs = zmax;
			}

			if(min_of_maxs.HasValue && max_of_mins.HasValue)
			{
				// Is min of maxs greater than max of mins?
				if(Vector3.Dot(min_of_maxs.Value - max_of_mins.Value, dir) > 0)
				{
					return true;
				}
			}

			return false;
		}
	}
}