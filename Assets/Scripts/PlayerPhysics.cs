using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysics : MonoBehaviour {

    public BranchManager branchManager;

    float speedValue = .02f;
    float horizontalSpeedValue = .001f;

    float angle = 0;

    int currentChunkIndex = 0;
    int prevChunkCount = 0;

    Branch currentBranch;
    int currentSplineIndex = 0;
    float currentSplineRatio = 0;
    Vector3 currentSplineStart = new Vector3(21, -21, 0);
    Vector3 currentSplineEnd = new Vector3(-21, 21, 0);

    float currentDistanceTraveled = 0;
    float currentBranchDistance = 0;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (currentBranch != null)
        {
            float horizontalInput = 0;
            if (Input.GetKey(KeyCode.LeftArrow)) horizontalInput = 1;
            else if (Input.GetKey(KeyCode.RightArrow)) horizontalInput = -1;

            float deltaTime = Time.deltaTime;

            float currentBranchLength = Vector3.Magnitude(currentSplineEnd - currentSplineStart);
            float deltaRatio = speedValue * deltaTime / currentBranchLength;
            currentSplineRatio += deltaRatio;
            float prevBranchLength = currentBranchLength;
            if (currentSplineRatio > 1)
            {
                if (currentSplineIndex == currentBranch.finalPoints.Length - 1)
                {
                    // If last spline, move to next Branch
                    currentBranch = null;
                }
                else
                {
                    currentSplineIndex++;
                    currentSplineStart = currentBranch.finalPoints[currentSplineIndex];
                    currentSplineEnd = currentBranch.finalPoints[currentSplineIndex + 1];
                }
                float splineRatioOverlap = currentSplineRatio - 1;
                float splineRatioBefore = deltaRatio - splineRatioOverlap;
                float usedUpSpeed = splineRatioBefore * prevBranchLength / deltaTime;
                float newSpeedValue = speedValue - usedUpSpeed;

                currentBranchLength = Vector3.Magnitude(currentSplineEnd - currentSplineStart);
                deltaRatio = newSpeedValue * deltaTime / currentBranchLength;
                currentSplineRatio = deltaRatio;
            }

            currentDistanceTraveled += deltaTime * speedValue;

            float radius = GetRadius(currentBranch, currentDistanceTraveled / currentBranchDistance);

            float circumference = 2 * Mathf.PI * (radius * radius);
            float angleRatio = horizontalSpeedValue * horizontalInput / circumference;
            angle += angleRatio * 360;

            transform.position = PositionOnBranch(currentSplineRatio, currentSplineStart, currentSplineEnd, radius, angle);
        }
        else
        {
            if (branchManager.chunks.Count > 0)
            {
                SetCurrentBranch(branchManager.chunks[0]);
            }
        }
	}

    public void SetCurrentBranch(Branch branch)
    {
        currentBranch = branch;
        currentSplineIndex = 0;
        currentSplineRatio = 0;
        currentSplineStart = currentBranch.finalPoints[currentSplineIndex];
        currentSplineEnd = currentBranch.finalPoints[currentSplineIndex + 1];
        currentDistanceTraveled = 0;
        currentBranchDistance = 0;
        for (int i = 0; i < currentBranch.finalPoints.Length - 1; i++)
        {
            float distance = Vector3.Magnitude(currentBranch.finalPoints[i + 1] - currentBranch.finalPoints[i]);
            currentBranchDistance += distance;
        }
    }

    public Vector3 PositionOnBranch(float distanceRatio, Vector3 start, Vector3 end, float radius, float angle)
    {
        Vector3 pos = Vector3.Lerp(start, end, distanceRatio);
        Vector3 forward = Vector3.Normalize(end - start);
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.up));
        Vector3 up = Vector3.Normalize(Vector3.Cross(right, forward));
        pos += up * radius;
        pos = Quaternion.AngleAxis(angle, forward) * pos;
        return pos;
    }

    public float GetRadius(Branch branch, float percent)
    {
        return branch.GetComponent<BranchMesh>().GetRadiusAt(percent);
    }
}
