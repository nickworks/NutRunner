using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[RequireComponent(typeof(Branch))]
public class BranchMesh : MonoBehaviour {

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
	public int sides = 8;
	public float radiusMax = 0.5f;
    public float radiusMin = 0.1f;
    public Vector2 scaleUV = new Vector2(1, 1);
    public AnimationCurve radiusCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0) });

    public void BuildMesh(BranchMesh parent = null)
    {
        line = GetComponent<Branch>(); // reference the Branch component
        Mesh mesh = GetComponent<MeshFilter>().mesh = new Mesh(); // make an empty mesh
        if (points != null && points.Length == 0) return; // can't make a mesh without points...

        if (parent) sides = parent.sides;

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
        rings = new Vector3[points.Length, sides + 1]; // sides + 1 (UV seams!)
        float arc = 2 * Mathf.PI / sides;
        Quaternion prevQuat = Quaternion.identity; // keep track of the angle of the previous ring...
        for (int i = 0; i < points.Length; i++)
        { // loop through all of the points in the spline...

            bool isFirst = (i == 0);
            bool isLast = (i == points.Length - 1);

            Quaternion nextQuat = prevQuat;
            if (!isLast)
            {
                // if the current point is NOT the end point...
                // get the quaternion to the next point:
                nextQuat = line.GetRotationAtPoint(i);
                // average the quaternion with the previous quaternion (if this point isn't the first point):
                prevQuat = isFirst ? nextQuat : Quaternion.Slerp(prevQuat, nextQuat, 0.5f);
            }
            Quaternion rotation = prevQuat;
            float angle = 0;
            
            for (int j = 0; j < sides; j++)
            {
                // get the next point in the ring:
                Vector3 v = radii[i] * new Vector3(0, Mathf.Sin(angle), Mathf.Cos(angle));
                // rotate the point, translate the point, store the point:
                rings[i, j] = rotation * v + points[i];
                // spin the angle:
                angle -= arc;
            }
            rings[i, sides] = rings[i, 0]; // copy the first point (for UV seams!)

            prevQuat = nextQuat;
        }

        /////////////////// OVERRIDE FIRST RING:
        // Copy the last ring from the parent BranchMesh:
        if (parent)
        {

            Vector3 meshOffset = parent.transform.position - transform.position;

            Vector3[] prevRing = parent.GetLastRing();
            for (int i = 0; i < prevRing.Length; i++)
            {
                rings[0, i] = prevRing[i] + meshOffset;
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
        int[] tris = new int[sides * (points.Length - 1) * 6];

        int triNum = 0;
        for (int i = 0; i < points.Length - 1; i++)
        {
            for (int j = 0; j < sides; j++)
            {

                int n = i * (sides + 1) + j; // sides + 1 (UV seams!)

                int cornerBottomRight = n;
                int cornerBottomLeft = n + 1;
                int cornerTopLeft = n + sides + 2;
                int cornerTopRight = n + sides + 1;

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
            for (int j = 0; j <= sides; j++)
            {
                float u = scaleUV.x * j / sides;
                float v = scaleUV.y * i / (points.Length - 1); // FIXME

                uvs[i * (sides + 1) + j] = new Vector2(-u, v);
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
            int ringNum = i / (sides + 1); // sides + 1 (UV seams!)
            normals[i] = (verts[i] - points[ringNum]).normalized; // FIXME: calculation is incorrect
        }
        return normals;
    }
    private Color[] GenerateColors(Vector3[] verts)
    {
        Color[] colors = new Color[verts.Length];
        for(int i = 0; i < colors.Length; i++)
        {
            float radius = radii[i / (sides + 1)];
            float percent = 1 - (radius - radiusMin) / (radiusMax - radiusMin);
            colors[i] = Color.Lerp(Color.black, Color.white, percent);
        }
        return colors;
    }
    public Vector3[] GetLastRing()
    {
        return GetRing(rings.GetLength(0) - 1);
    }
    public Vector3[] GetRing(int num)
    {
        if (num < 0 || num >= rings.GetLength(0)) return new Vector3[0];
   
        int count = rings.GetLength(1);
        Vector3[] result = new Vector3[count];
        for(int i = 0; i < count; i++)
        {
            result[i] = rings[num,i];
        }
        return result;
    }
    public float GetRadiusAt(float percent)
    {
        percent = Mathf.Clamp(percent, 0, 1);
        return Mathf.Lerp(radiusMin, radiusMax, radiusCurve.Evaluate(percent));
    }
}
