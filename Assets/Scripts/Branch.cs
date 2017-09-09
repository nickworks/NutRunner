using UnityEngine;
using System.Collections.Generic;

public class Branch : MonoBehaviour {

    [System.Serializable]
    public struct SubdivisionSettings
    {
        [Range(0.1f, 1f)] public float threshold;
        [Range(0.0f, 1f)] public float jitterAmount;
    }

    /// <summary>
    /// This is an array of vertices that makes up a line.
    /// </summary>
	public Vector3[] points { get; private set; }
    /// <summary>
    /// This is the array of vertices to use after subdiving the line.
    /// </summary>
    public Vector3[] finalPoints { get; private set; }

    public float length = 1;
    public SubdivisionSettings[] subdivisionSettings;

    public BranchMesh mesh { get; private set; }

    public void Init(Branch parent = null)
    {
        mesh = GetComponent<BranchMesh>();
        if (parent) transform.position = parent.GetEndPoint();

        GeneratePoints(parent);
        
        mesh.BuildMesh((parent) ? parent.mesh : null);
    }

    private void GeneratePoints(Branch parent)
    {

        Quaternion oldAngle = (parent) ? parent.GetRotationAtPoint(100) : Quaternion.identity;
        Quaternion newAngle = Random.rotationUniform;

        List<Vector3> temp = new List<Vector3>();
        temp.Add(new Vector3(0, 0, 0));
        int max = 10;
        for (int i = 0; i <= max; i++) {
            Quaternion angleToGrow = Quaternion.Slerp(oldAngle, newAngle, i/(float)max);
            Vector3 p = angleToGrow * Vector3.right * length + temp[temp.Count - 1];
            temp.Add(p);
        }

        points = temp.ToArray();
        Subdivide();
    }

    /// <summary>
    /// Returns the world-coordinates of the last point.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetEndPoint()
    {
        return finalPoints[finalPoints.Length - 1] + transform.position;
    }
    public Quaternion GetRotationAtPoint(int n)
    {
        if (n < 0) n = 0;
        if (n >= finalPoints.Length - 1) n = finalPoints.Length - 2;
        Vector3 delta = finalPoints[n + 1] - finalPoints[n];

        return Quaternion.FromToRotation(transform.right, delta);
    }
    public Vector3 GetDirectionToNextPoint(int n)
    {
        if (n < 0) n = 0;
        if (n >= finalPoints.Length - 1) n = finalPoints.Length - 2;
        Vector3 delta = finalPoints[n + 1] - finalPoints[n];

        return delta;
    }
    public Quaternion GetRotationOffset(int n)
    {

        Vector3 dir1 = GetDirectionToNextPoint(n - 1);
        Vector3 dir2 = GetDirectionToNextPoint(n);

        return Quaternion.FromToRotation(dir1, dir2);
    }
    private void Subdivide()
    {
        List<Vector3> temp = new List<Vector3>(points);

        for (int pass = 0; pass < subdivisionSettings.Length; pass++)
        {
            for (int i = 1; i < temp.Count; i++)
            {
                Vector3 curr = temp[i];
                Vector3 prev = temp[i - 1];

                float dis = (curr - prev).magnitude;
                float threshold = subdivisionSettings[pass].threshold;
                if (dis > threshold && threshold >= 0.1f)
                {
                    Vector3 avg = 0.5f * (curr + prev) + GetJitter(subdivisionSettings[pass].jitterAmount);
                    temp.Insert(i, avg);
                    i--;
                }
            }
        }

        finalPoints = temp.ToArray();
    }
    /// <summary>
    /// This method is deprecated.
    /// </summary>
    private void BakeSubdivision()
    {
        //if (!doSubdivision) return;
        points = (Vector3[]) finalPoints.Clone();
        //doSubdivision = false;
    }
    private float GetMag()
    {
        float total = 0;
        for (int i = 1; i < points.Length; i++)
        {
            Vector3 prev = points[i - 1];
            Vector3 curr = points[i];
            total += (curr - prev).magnitude;
        }
        return total;
    }
    public Vector3 GetLerpPosition(int n, float p)
    {
        if (n < 0) n = 0;
        if (n >= finalPoints.Length) n = finalPoints.Length - 1;

        Vector3 p1 = finalPoints[n];
        Vector3 p2 = (n >= finalPoints.Length - 1) ? p1 : finalPoints[n + 1];

        return Vector3.Lerp(p1, p2, p);
    }
    public Vector3 GetJitter(float amt)
    {
        return 0.5f * new Vector3(
            Random.Range(-amt, amt),
            Random.Range(-amt, amt),
            Random.Range(-amt, amt)
            );
    }
}
