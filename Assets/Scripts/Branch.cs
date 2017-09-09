using UnityEngine;
using System.Collections.Generic;

public class Branch : MonoBehaviour {

    /// <summary>
    /// This is an array of vertices that makes up a line.
    /// </summary>
	public Vector3[] points = new Vector3[] {};
    /// <summary>
    /// This is the array of vertices to use after subdiving the line.
    /// </summary>
    public Vector3[] finalPoints { get; private set; }
    public int seed = 0;
    public bool doSubdivision = true;
    public float growAmount = 0.1f;
    [Range(0.1f, 1f)] public float subdivideThreshold = .25f;
    [Range(0.0f, 1f)] public float jitterAmount = .1f;

    public void Init(Branch parent = null)
    {
        Quaternion angleToGrow = (parent) ? parent.GetRotationAtPoint(100) : Quaternion.identity;

        if (parent) transform.position = parent.GetEndPoint();

        
        Vector3 p1 = new Vector3(0, 0, 0);
        Vector3 p2 = angleToGrow * Vector3.right;
        //Vector3 p3 = new Vector3(0, 1.5f, .5f);

        points = new Vector3[] { p1, p2 };

        UpdateSpline();

        BranchMesh parentMesh = (parent) ? parent.GetComponent<BranchMesh>() : null;
        GetComponent<BranchMesh>().BuildMesh(parentMesh);
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
        if (n >= finalPoints.Length) n = finalPoints.Length - 2;
        Vector3 ptFrom = finalPoints[n];
        Vector3 ptTo = finalPoints[n + 1];
        return Quaternion.FromToRotation(Vector3.right, ptTo - ptFrom);
    }
    private void UpdateSpline()
    {
        Random.InitState(seed);
        if (doSubdivision)
        {
            Subdivide();
        }
        else
        {
            finalPoints = (Vector3[])points.Clone();
        }
    }
    private void Subdivide()
    {
        List<Vector3> temp = new List<Vector3>(points);

        for(int i = 1; i < temp.Count; i++)
        {
            Vector3 curr = temp[i];
            Vector3 prev = temp[i - 1];

            float dis = (curr - prev).magnitude;

            if(dis > subdivideThreshold)
            {
                Vector3 avg = 0.5f * (curr + prev) + GetJitter();
                temp.Insert(i, avg);
                i--;
            }
        }

        finalPoints = temp.ToArray();
    }
    private void BakeSubdivision()
    {
        if (!doSubdivision) return;
        points = (Vector3[]) finalPoints.Clone();
        doSubdivision = false;
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
    private Vector3 GetJitter()
    {
        return 0.5f * new Vector3(
            Random.Range(-jitterAmount, jitterAmount),
            Random.Range(-jitterAmount, jitterAmount),
            Random.Range(-jitterAmount, jitterAmount)
            );
    }
}
