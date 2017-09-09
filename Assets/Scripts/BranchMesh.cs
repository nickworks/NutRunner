using UnityEngine;
//using UnityEditor;
using System.Collections.Generic;

[RequireComponent(typeof(Branch))]
public class BranchMesh : MonoBehaviour {

    public const int SIDES = 16;

    Branch line;
    public Vector3[] points
    {
        get
        {
            if (!line) return new Vector3[0];
            return line.finalPoints;
        }
    }
    private float[] radii;
    /// <summary>
    /// The vertices that form "rings" around the initial line.
    /// The 1st dimension refers to the specific ring.
    /// The 2nd dimension refers to points within that ring.
    /// </summary>
    private Vector3[,] rings = new Vector3[0,0];
	
	public float radiusMax = 0.5f;
    public float radiusMin = 0.1f;
    public Vector2 scaleUV = new Vector2(1, 1);
    public AnimationCurve radiusCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0) });

    public void BuildMesh(BranchMesh parent = null)
    {
        line = GetComponent<Branch>(); // reference the Branch component
        Mesh mesh = GetComponent<MeshFilter>().mesh = new Mesh(); // make an empty mesh
        if (points != null && points.Length == 0) return; // can't make a mesh without points...

        //if (parent) SIDES = parent.SIDES;

        GenerateRadii(parent);
        GenerateRings(parent);
        
        mesh.vertices = GenerateVertList();
        mesh.triangles = GenerateTris(mesh.vertices);
        mesh.uv = GenerateUVs(mesh.vertices);
        mesh.normals = GenerateNormals(mesh.vertices);
        mesh.colors = GenerateColors(mesh.vertices);
    }
    /// <summary>
    /// This method figures out the appropriate radii for all segments
    /// based on this object's radiusCurve.
    /// </summary>
    private void GenerateRadii(BranchMesh parent)
    {
        // if there's a parent mesh, use it's ending radius:
        if (parent) radiusMax = parent.GetRadiusAt(1); 

        radii = new float[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            float percent = i / (float)(points.Length - 1);
            radii[i] = GetRadiusAt(percent);
        }
    }
    /// <summary>
    /// This method generates the rings of points that form the mesh that encircles the Line.
    /// </summary>
    private void GenerateRings(BranchMesh parent)
    {
        rings = new Vector3[points.Length, SIDES + 1]; // sides + 1 (UV seams!)
        

        for (int i = 0; i < points.Length; i++)
        { // loop through all of the points in the spline...
            if(i == 0)
            {
                if (parent) CopyFirstRingFromParent(parent);
                else GenerateFirstRing();
            } else
            {
                CopyAndRotateRing(i);
            }
        }
        Jitter();
    }
    /// <summary>
    /// This method copies the last ring from the parent mesh and uses the data to create this mesh's first ring.
    /// </summary>
    private void CopyFirstRingFromParent(BranchMesh parent)
    {
        if (parent)
        {
            Vector3 meshOffset = parent.transform.position - transform.position;
            Vector3[] prevRing = parent.GetLastRing();
            SetRing(0, prevRing, meshOffset);
        }
    }
    /// <summary>
    /// This method creates ring #n by copying the previous ring and rotating it slightly.
    /// </summary>
    private void CopyAndRotateRing(int n)
    {
        Quaternion rot = line.GetRotationOffset(n);
        Vector3[] temp = GetRing(n - 1, true);
        for (int i = 0; i < temp.Length; i++) temp[i] = rot * temp[i];
        SetRing(n, temp, points[n]);
    }
    /// <summary>
    /// This method generates a ring of vertices and uses it to create the first ring in this mesh.
    /// </summary>
    private void GenerateFirstRing()
    {
        float arc = 2 * Mathf.PI / SIDES;
        float angle = 0;
        for (int j = 0; j < SIDES; j++)
        {
            // get the next point in the ring:
            Vector3 v = radii[0] * new Vector3(0, Mathf.Sin(angle), Mathf.Cos(angle));
            // rotate the point, translate the point, store the point:
            rings[0, j] = v + points[0];
            // spin the angle:
            angle -= arc;
        }
        rings[0, SIDES] = rings[0, 0]; // copy the first point (for UV seams!)
    }
    private void SetRing(int n, Vector3[] v, Vector3 offset)
    {
        if (n < 0 || n >= rings.GetLength(0)) return;
        for (int i = 0; i < v.Length; i++)
        {
            rings[n, i] = v[i] + offset;
        }
    }
    private void Jitter()
    {
        for(int i = 1; i < rings.GetLength(0) - 1; i++)
        {
            for (int j = 1; j < rings.GetLength(1) - 1; j++)
            {
                rings[i,j] += line.GetJitter(0.1f);
            }
        }
    }
    /// <summary>
    /// This method generates vertex lists for the final mesh.
    /// </summary>
    /// <returns>The generated list of vertices.</returns>
    private Vector3[] GenerateVertList()
    {

        int num1 = rings.GetLength(0);
        int num2 = rings.GetLength(1);
        Vector3[] vertices = new Vector3[num1 * num2];

        int k = 0;
        for(int i = 0; i < num1; i++)
        {
            for (int j = 0; j < num2; j++)
            {
                vertices[k] = rings[i, j];
                k++;
            }
        }
        return vertices;
    }
    /// <summary>
    /// This method generates the tris that make up the final mesh.
    /// </summary>
    /// <param name="verts">A vertex list of the mesh's vertices.</param>
    /// <returns>An array of index numbers which correspond to values in the verts array.</returns>
    private int[] GenerateTris(Vector3[] verts)
    {
        int[] tris = new int[SIDES * (points.Length - 1) * 6];

        int triNum = 0;
        for (int i = 0; i < points.Length - 1; i++)
        {
            for (int j = 0; j < SIDES; j++)
            {

                int n = i * (SIDES + 1) + j; // sides + 1 (UV seams!)

                int cornerBottomRight = n;
                int cornerBottomLeft = n + 1;
                int cornerTopLeft = n + SIDES + 2;
                int cornerTopRight = n + SIDES + 1;

                tris[triNum++] = cornerBottomRight;
                tris[triNum++] = cornerBottomLeft;
                tris[triNum++] = cornerTopRight;

                tris[triNum++] = cornerTopRight;
                tris[triNum++] = cornerBottomLeft;
                tris[triNum++] = cornerTopLeft;
            }
        }
        return tris;
    }
    /// <summary>
    /// This method generates a list of UVs.
    /// </summary>
    /// <param name="verts">A vertex list. Each UV generated will corespond with a specific vertex.</param>
    /// <returns>The array of generated UVs.</returns>
    private Vector2[] GenerateUVs(Vector3[] verts)
    {

        Vector2[] uvs = new Vector2[verts.Length];
        for (int i = 0; i <= points.Length - 1; i++)
        {
            for (int j = 0; j <= SIDES; j++)
            {
                float u = scaleUV.x * j / SIDES;
                float v = scaleUV.y * i / (points.Length - 1); // FIXME

                uvs[i * (SIDES + 1) + j] = new Vector2(-u, v);
            }
        }
        return uvs;
    }
    /// <summary>
    /// This function generates vertex normals for an array of vertices that roughly makeup a cylinder.
    /// </summary>
    /// <param name="verts">The array of vertices to generate normals for.</param>
    /// <returns></returns>
    private Vector3[] GenerateNormals(Vector3[] verts)
    {
        Vector3[] normals = new Vector3[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            int ringNum = i / (SIDES + 1); // sides + 1 (UV seams!)
            normals[i] = (verts[i] - points[ringNum]).normalized; // FIXME: calculation is incorrect
        }
        return normals;
    }
    private Color[] GenerateColors(Vector3[] verts)
    {
        Color[] colors = new Color[verts.Length];
        for(int i = 0; i < colors.Length; i++)
        {
            float radius = radii[i / (SIDES + 1)];
            float percent = 1 - (radius - radiusMin) / (radiusMax - radiusMin);
            colors[i] = Color.Lerp(Color.black, Color.white, percent);
        }
        return colors;
    }
    public Vector3[] GetLastRing()
    {
        return GetRing(rings.GetLength(0) - 1);
    }
    public Vector3[] GetRing(int num, bool shiftToOrigin = false)
    {
        if (num < 0 || num >= rings.GetLength(0)) return new Vector3[0];
   
        int count = rings.GetLength(1);
        Vector3[] result = new Vector3[count];
        for(int i = 0; i < count; i++)
        {
            result[i] = rings[num, i];
            if(shiftToOrigin) result[i] -= points[num];
        }
        return result;
    }
    public float GetRadiusAt(float percent)
    {
        percent = Mathf.Clamp(percent, 0, 1);
        return Mathf.Lerp(radiusMin, radiusMax, radiusCurve.Evaluate(percent));
    }

    public struct BranchSurfaceProperties
    {
        public Vector3 position;
        public Vector3 up;
        public Vector3 forward;
    }

    public BranchSurfaceProperties GetSurfaceProperties(int segment, float segmentPercent, int lane, float lanePercent)
    {
        int maxSegment = rings.GetLength(0) - 1;
        int maxLane = rings.GetLength(1) - 1;

        if (segment < 0) segment = 0;
        if (segment >= maxSegment) segment = maxSegment;

        if (lane < 0) lane = 0;
        if (lane >= maxLane) lane = 0;

        int segment2 = segment + 1;
        if (segment2 >= maxSegment) segment2 = maxSegment;

        int lane2 = lane + 1;

        Vector3 p1 = Vector3.Lerp(rings[segment,  lane], rings[segment,  lane2], lanePercent); // the point behind the player
        Vector3 p2 = Vector3.Lerp(rings[segment2, lane], rings[segment2, lane2], lanePercent); // the point in front of the player

        BranchSurfaceProperties results = new BranchSurfaceProperties();
        results.position = Vector3.Lerp(p1, p2, segmentPercent);
        results.forward = (p2 - p1).normalized;
        results.up = (results.position - line.GetLerpPosition(segment, segmentPercent)).normalized;

        results.position += transform.position;

        return results;
    }
}
