using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class OBBTree : MonoBehaviour
{
	// Root of the OBBTree. Used as a starting point when parsing the tree
	protected OBBTreeNode root;

    private Mesh mesh;

    // Array with mesh vertices 
    Vector3[] vertices;

    // index of vertex on each triangle of the mesh
    int[] triangles;

    // candidates triangles for intersection with ray 
    List<int> candidatesTriangles = new List<int>(); 
    
    // Store the recursivity level of OBB to be drawn
    // Use -1 to disable OBB drawing
    [Range(-1, 100)]
	[SerializeField]
	protected int levelToBeDrawn = -1;

     // max itter 
     private int max_itter = 15;
     private int n = 0; 

    public void Start()
    {
        mesh = this.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        triangles = mesh.triangles;

        Vector3 mu = meanIsoBarycenter();

        Matrix3x3 C = computeCovMatrix(mu);

        List<Vector3> extremas = seekExtremaOOB(C.Transposed());

        OBB obb = computeOBB(extremas);

        root.obb = obb;
        root.vertices = vertices;
        root.triangles = triangles;
    }

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

    // compute barycente of triangle 
    
    public Vector3 meanIsoBarycenter()
    {
        int N = triangles.Length;

        float mux = 0;
        float muy = 0;
        float muz = 0; 

        for (int i=0; i< N; i+=3)
        {
            Vector3 p = vertices[triangles[i]];
            Vector3 q = vertices[triangles[i + 1]]; 
            Vector3 r = vertices[triangles[i + 2]];

            mux = mux + p.x + q.x + r.x;
            muy = muy + p.y + q.y + r.y;
            muz = muz + p.z + q.z + r.z; 
        }
        mux = mux / (3 * N);
        muy = muy / (3 * N);
        muz = mux / (3 * N);

        return new Vector3(mux, muy, muz);
    }

    public Matrix3x3 computeCovMatrix(Vector3 mu)
    {
        int N = triangles.Length;

        Matrix3x3 cov = new Matrix3x3();

        for (int i = 0; i < N; i += 3)
        {
            Vector3 p_ = vertices[triangles[i]] - mu;
            Vector3 q_ = vertices[triangles[i + 1]] - mu;
            Vector3 r_ = vertices[triangles[i + 2]] - mu;

            for (int j = 0; j < 3; j++)
            {

                for (int k = 0; k < 3; k++)
                {
                    cov[j, k] = cov[j, k] + (p_[j] * p_[k] + q_[j] * q_[j] + r_[j] * r_[k]) * (1 / (3 * N));
                }
            }
        }
        return cov;
    }

    public List<Vector3> seekExtremaOOB(Matrix3x3 covtransposed)
    {
        List<Vector3> extrema = new List<Vector3>();

        Vector3[] eigenVectors = covtransposed.EigenVectors();

        Vector3 mini = new Vector3();
        Vector3 maxi = new Vector3();

        // min 
        extrema.Add(vertices[0]);

        // max
        extrema.Add(vertices[0]);

        Matrix3x3 newBase = new Matrix3x3(); 

        for (int i = 0; i < eigenVectors.Length; i++)
        {
            Vector3 vec = eigenVectors[i];
            vec = vec.normalized;

            newBase.SetColumn(i, vec); 
        }

        // get new coords  
        int N = vertices.Length;

        for (int i=0; i<N; i++)
        {
            vertices[i] = newBase * vertices[i];

            Vector3 vec = vertices[i]; 

            for (int j = 0; j < 3; j++)
            {
                // min 
                if (vec[j] < mini[j])
                {
                    mini[j] = vec[j];
                }
     
                // max 
                if (vec[j] > maxi[j])
                {
                    maxi[j] = vec[j];
                }
            }
        }
        extrema[0] = mini;
        extrema[1] = maxi;

        return extrema;
    }

    public OBB computeOBB(List<Vector3> extremas)
    {
        OBB obb = new OBB();
        Vector3 diag = extremas[1] - extremas[0];

        // 
        if(diag.x < diag.y)
        {
            if(diag.x < diag.z)
            {
                obb.shortAxis = OBB.Axis.X;
                if(diag.y < diag.z)
                {
                    obb.medAxis = OBB.Axis.Y;
                    obb.longAxis = OBB.Axis.Z;
                }
                else
                {
                    obb.medAxis = OBB.Axis.Z;
                    obb.longAxis = OBB.Axis.Y;
                }
            }
            else
            {
                obb.medAxis = OBB.Axis.X;
                obb.shortAxis = OBB.Axis.Z;
                obb.longAxis = OBB.Axis.Y;
            }
        }
        else
        {
            if(diag.y < diag.z)
            {
                obb.shortAxis = OBB.Axis.Y;
                if (diag.x < diag.z)
                {
                    obb.medAxis = OBB.Axis.X;
                    obb.longAxis = OBB.Axis.Z;
                }
                else
                {
                    obb.medAxis = OBB.Axis.Z;
                    obb.longAxis = OBB.Axis.X;
                }
            }
            else
            {
                obb.medAxis = OBB.Axis.Y;
                obb.shortAxis = OBB.Axis.Z;
                obb.longAxis = OBB.Axis.X;
            }
        }

        Bounds obbbounds = new Bounds(extremas[0]+diag/2, diag);

        obb.bounds = obbbounds;

        Quaternion q = Quaternion.LookRotation(new Vector3(0, 0, diag.z), new Vector3(0, diag.y, 0));

        obb.orientation = q; 







        return obb;
    }

    public int[] getTrianglesInOBB(OBBTreeNode parentNode, Bounds bound)
    {
        for (int i = 0; i < parentNode.triangles.Length; i++)
        {
            
        }
    }

    public OBBTreeNode nextNode(OBBTreeNode parentNode)
    {
        n++;
        OBBTreeNode node = new OBBTreeNode();
        if (n > max_itter)
        {
            return null; 
        }
        else
        {
        
            Vector3 centerleft = parentNode.obb.bounds.center;
            centerleft[(int)parentNode.obb.longAxis] -=  0.25f * parentNode.obb.bounds.size[(int)parentNode.obb.longAxis];

            Vector3 centerright = parentNode.obb.bounds.center;
            centerright[(int)parentNode.obb.longAxis] += 0.25f * parentNode.obb.bounds.size[(int)parentNode.obb.longAxis];

            Vector3 size = parentNode.obb.bounds.size;
            size[(int)parentNode.obb.longAxis] /= 2;

            Bounds boundLeft = new Bounds(centerleft, size);
            Bounds boundRight = new Bounds(centerright, size);

            int N = parentNode.triangles.Length;

            for (int i = 0; i < N; i += 3)
            {
                Vector3 p = vertices[triangles[i]];
                Vector3 q = vertices[triangles[i + 1]];
                Vector3 r = vertices[triangles[i + 2]];

                               

            }

            /*Vector3 mu = meanIsoBarycenter();

            Matrix3x3 C = computeCovMatrix(mu);

            List<Vector3> extremas = seekExtremaOOB(C.Transposed());

            node.obb = computeOBB(extremas);*/



            // left node
            node.childrenNodes[0].obb =    ;
            node.childrenNodes[0].triangles = ; 


            //right node
            node.childrenNodes[1].obb =  ; 


            //OBB obb = ;


            // tester equation plan avec les ; 

        }

        return nextNode();
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
