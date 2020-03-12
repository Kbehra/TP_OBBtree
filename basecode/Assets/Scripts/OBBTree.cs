using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class OBBTree : MonoBehaviour
{
    #region Fields
    // Root of the OBBTree. Used as a starting point when parsing the tree
    protected OBBTreeNode root;

    // Store all the mesh vertices
    protected Vector3[] vertices;

    // Store vertices ids of all the triangles
    protected int[] indices;

    // Store all the triangle centroïds
    protected Vector3[] centers;

    // Store the mean of the centroïds of the current triangles set
    protected Vector3 mean;

    // Store all the triangles ids contained in OBBs that are intersected by the ray
    protected HashSet<int> potentiallyIntersectedTriangles = new HashSet<int>();

    // Store the recursivity level of OBB to be drawn
    // Use -1 to disable OBB drawing
    [Range(-1, 100)]
    [SerializeField]

    protected int levelToBeDrawn = -1;
    #endregion

    #region MonoBehaviour callbacks
    void Start()
    {
        Build();
    }

    void Update()
    {
        if (levelToBeDrawn > -1)
        {
            Draw(levelToBeDrawn);
        }
    }
    #endregion

    #region Internal methods
    /// <summary>
    /// Entry point for building OBBTree
    /// </summary>
    protected void Build()
    {
        // Gets the whole mesh vertices
        // vertices[vertex_id] gives the position (in object space) of vertex number vertex_id
        vertices = GetComponent<MeshFilter>().mesh.vertices;

        // Gets the vertex ids of all the triangles
        indices = GetComponent<MeshFilter>().mesh.triangles;

        // Compute triangle count
        int triangle_count = indices.Length / 3;

        // triangles array stores triangles ids (i.e. 1 int per triangle)
        int[] triangles = new int[triangle_count];

        // Initialize triangles array with all the triangle ids
        for (int i = 0; i < triangle_count; i++)
        {
            triangles[i] = i;
        }

        // Precomputes triangles centroïds
        ComputeCenters(triangles);

        // Create OBBTree root
        root = new OBBTreeNode();

        // Compute OBB for root node, i.e. for every triangle in the mesh
        root.obb = ComputeOBB(triangles);

        // Start recursive subdivision
        DivideRecursively(root, triangles);
    }

    // Recursive method that splits an OBB and compute the OBBs of the sub-spaces
    // If OBB can not be split, stores triangle ids contained in leaf node
    protected void DivideRecursively(OBBTreeNode node, int[] triangles)
    {
        int[] side1;
        int[] side2;

        Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward };

        // First try to split longest axis, then medium, and then smallest
        if (Split(triangles, node.obb.orientation * axes[(int)node.obb.longAxis], out side1, out side2) ||
            Split(triangles, node.obb.orientation * axes[(int)node.obb.medAxis], out side1, out side2) ||
            Split(triangles, node.obb.orientation * axes[(int)node.obb.shortAxis], out side1, out side2))
        {
            triangles = null;

            // Create node for sub-space 1
            node.childrenNodes[0] = new OBBTreeNode();

            // Compute OBB of triangles contained in sub-space 1
            node.childrenNodes[0].obb = ComputeOBB(side1);

            // Call same method sub-space 1
            DivideRecursively(node.childrenNodes[0], side1);

            // Create node for sub-space 2
            node.childrenNodes[1] = new OBBTreeNode();

            // Compute OBB of triangles contained in sub-space 2
            node.childrenNodes[1].obb = ComputeOBB(side2);

            // Call same method on sub-space 2
            DivideRecursively(node.childrenNodes[1], side2);
        }
        else
        {
            // Store triangles ids contained in OBB
            node.obb.containedTriangles = new HashSet<int>(triangles);
        }
    }

    // Split a triangle set with respect to a plane orthogonal to an axis and located at the mean of every triangle centroïd
    protected bool Split(int[] triangles, Vector3 plane_normal, out int[] side1, out int[] side2)
    {
        Vector3 plane_point = mean;

        // Compute plane parms
        float a = plane_normal.x;
        float b = plane_normal.y;
        float c = plane_normal.z;
        float d = -(a * plane_point.x + b * plane_point.y + c * plane_point.z);

        List<int> s1 = new List<int>();
        List<int> s2 = new List<int>();

        // Sub-division iff side 1 and side 2 are not empty after sub-division
        bool side1_is_empty = true;
        bool side2_is_empty = true;

        // For each triangle in the triangle set
        for (int i = 0; i < triangles.Length; i++)
        {
            // Retrieve vertices positions
            Vector3 v1 = vertices[indices[3 * triangles[i] + 0]];
            Vector3 v2 = vertices[indices[3 * triangles[i] + 1]];
            Vector3 v3 = vertices[indices[3 * triangles[i] + 2]];

            // Compute value of the plane equation for every vertex
            float p1 = a * v1.x + b * v1.y + c * v1.z + d;
            float p2 = a * v2.x + b * v2.y + c * v2.z + d;
            float p3 = a * v3.x + b * v3.y + c * v3.z + d;

            // If the whole triangle lies on the same side of the plane
            if (p1 > 0 && p2 > 0 && p3 > 0)
            {
                s1.Add(triangles[i]);

                side1_is_empty = false;
            }
            else if (p1 < 0 && p2 < 0 && p3 < 0)
            {
                s2.Add(triangles[i]);

                side2_is_empty = false;
            }
            else
            {
                // If some vertices are on the one side and others are the other side,
                // add triangles to both sides
                s1.Add(triangles[i]);

                s2.Add(triangles[i]);
            }
        }

        side1 = s1.ToArray();
        side2 = s2.ToArray();

        return !side1_is_empty && !side2_is_empty;
    }

    // Compute triangles centroïds
    protected Vector3[] ComputeCenters(int[] triangles)
    {
        centers = new Vector3[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            Vector3 sum = Vector3.zero;

            for (int j = 0; j < 3; j++)
            {
                sum += vertices[indices[3 * triangles[i] + j]];
            }

            centers[i] = sum / 3.0f;
        }

        return centers;
    }

    // Compute mean of triangles centroïds
    protected Vector3 ComputeMean(int[] triangles)
    {
        int size = triangles.Length;

        Vector3 sum = Vector3.zero;

        for (int i = 0; i < size; i++)
        {
            sum += centers[triangles[i]];
        }

        return sum / (float)size;
    }

    // Compute co-variance matrix
    protected Matrix3x3 ComputeCovariance(int[] triangles)
    {
        mean = ComputeMean(triangles);

        Matrix3x3 covariance = new Matrix3x3();

        for (int j = 0; j < 3; j++)
        {
            for (int k = 0; k < 3; k++)
            {
                float cov = 0.0f;

                int size = triangles.Length;

                for (int i = 0; i < size; i++)
                {
                    Vector3 p1 = vertices[indices[3 * triangles[i] + 0]] - mean;
                    Vector3 p2 = vertices[indices[3 * triangles[i] + 1]] - mean;
                    Vector3 p3 = vertices[indices[3 * triangles[i] + 2]] - mean;

                    cov += p1[j] * p1[k] + p2[j] * p2[k] + p3[j] * p3[k];
                }

                cov /= 3 * triangles.Length;

                covariance[j, k] = cov;

                if (j != k)
                {
                    covariance[k, j] = cov;
                }
            }
        }

        return covariance;
    }

    // Compute min and max points in OBB coordinates
    protected void FindMinMax(int[] triangles, Matrix3x3 eigen_matrix_transposed, out Vector3 min, out Vector3 max)
    {
        min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        for (int i = 0; i < triangles.Length; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector3 p = eigen_matrix_transposed * vertices[indices[3 * triangles[i] + j]];

                if (p.x < min.x)
                {
                    min.x = p.x;
                }
                if (p.y < min.y)
                {
                    min.y = p.y;
                }
                if (p.z < min.z)
                {
                    min.z = p.z;
                }

                if (p.x > max.x)
                {
                    max.x = p.x;
                }
                if (p.y > max.y)
                {
                    max.y = p.y;
                }
                if (p.z > max.z)
                {
                    max.z = p.z;
                }
            }
        }
    }

    // Compute OBB for a set of triangles
    protected OBB ComputeOBB(int[] triangles)
    {
        // Compute covariance matrix
        Matrix3x3 covariance = ComputeCovariance(triangles);

        // Compute covariance matrix eigen vectors
        Vector3[] eigen_vectors = covariance.EigenVectors();

        // Important: ensure base made of eigen vectors is direct
        eigen_vectors[2] = Vector3.Cross(eigen_vectors[0], eigen_vectors[1]);

        // Compute matrix made of eigen vectors,
        // which is the transformation matrix from local coordinates to OBB coordinates
        Matrix3x3 eigen_matrix = new Matrix3x3();
        for (int v = 0; v < 3; v++)
        {
            eigen_vectors[v].Normalize();

            eigen_matrix.SetColumn(v, eigen_vectors[v]);
        }

        // Compute the inverse transformation matrix (eigen_matrix is orthogonal)
        // Will be used to compute local to OBB coordinates change
        Matrix3x3 eigen_matrix_transposed = eigen_matrix.Transposed();

        // Compute min and max along OBB coordinates
        Vector3 min, max;

        FindMinMax(triangles, eigen_matrix_transposed, out min, out max);

        // Stores max, med and min axes
        // Will be used for OBB subdivision
        Vector3 e = max - min;

        OBB.Axis min_axis = OBB.Axis.X;
        float min_component = e[0];

        OBB.Axis long_axis = OBB.Axis.X;
        float max_component = e[0];

        for (OBB.Axis i = OBB.Axis.Y; i < OBB.Axis.Count; i++)
        {
            if (e[(int)i] < min_component)
            {
                min_component = e[(int)i];
                min_axis = i;
            }

            if (e[(int)i] > max_component)
            {
                max_component = e[(int)i];
                long_axis = i;
            }
        }

        OBB.Axis med_axis = (OBB.Axis)(((int)OBB.Axis.Count) - ((int)long_axis) - ((int)min_axis));

        // Create the actual OBB
        OBB obb = new OBB();

        obb.orientation = Quaternion.LookRotation(eigen_vectors[2], eigen_vectors[1]);

        obb.bounds = new Bounds();
        obb.bounds.SetMinMax(min, max);

        obb.longAxis = long_axis;
        obb.medAxis = med_axis;
        obb.shortAxis = min_axis;

        return obb;
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

    // Compute ray intersection with a node OBB
    protected void IntersectRay(Ray ray, OBBTreeNode node)
    {
        //DrawOBB(node.obb, Color.grey);

        if (node.obb.IntersectRay(ray))
        {
            // Add every triangle contained int the OBB (if any)
            // to the list of potentially intersected triangles
            if (node.obb.containedTriangles != null)
            {
                potentiallyIntersectedTriangles.UnionWith(node.obb.containedTriangles);
            }

            // Draw the intersected OBB in red
            //DrawOBB(node.obb, Color.red);

            // Call the same method on children
            foreach (OBBTreeNode child in node.childrenNodes)
            {
                if (child != null)
                {
                    IntersectRay(ray, child);
                }
            }
        }
    }
    #endregion

    #region Public methods
    public bool IntersectRay(Ray ray, out Vector3 closest_intersection_pt)
    {
        Ray ray_obj = new Ray(transform.InverseTransformPoint(ray.origin), transform.InverseTransformDirection(ray.direction));

        Vector3 origin = ray_obj.origin;

        Vector3 extremity = ray_obj.origin + 1000.0f * ray_obj.direction;

        potentiallyIntersectedTriangles.Clear();

        IntersectRay(ray_obj, root);

        bool intersection_exists = false;

        closest_intersection_pt = extremity;

        foreach (int i in potentiallyIntersectedTriangles)
        {
            // Triangle i vertices
            Vector3 v1 = vertices[indices[3 * i + 0]];
            Vector3 v2 = vertices[indices[3 * i + 1]];
            Vector3 v3 = vertices[indices[3 * i + 2]];

            Vector3 intersection_pt;

            if (Utils.UnityGeometry.GetSegmentTriangleIntersection(v1, v2, v3, origin, extremity, out intersection_pt))
            {
                intersection_exists = true;

                if ((intersection_pt - origin).sqrMagnitude < (closest_intersection_pt - origin).sqrMagnitude)
                {
                    closest_intersection_pt = intersection_pt;
                }
            }
        }

        closest_intersection_pt = transform.TransformPoint(closest_intersection_pt);

        return intersection_exists;
    }
    #endregion
}
