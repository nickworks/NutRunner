using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysics : MonoBehaviour {

    float speedValue = 1;
    float horizontalSpeedValue = 1;

    float angle = 0;

    float currentBranchRatio = 0;
    Vector3 currentBranchStart = new Vector3(21, -21, 0);
    Vector3 currentBranchEnd = new Vector3(-21, 21, 0);

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float horizontalInput = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) horizontalInput = 1;
        else if (Input.GetKey(KeyCode.RightArrow)) horizontalInput = -1;

        Debug.Log(horizontalInput);

        float radius = GetRadius();

        float circumference = 2 * Mathf.PI * (radius * radius);
        float angleRatio = horizontalSpeedValue * horizontalInput / circumference;
        angle += angleRatio * 360;

        float currentBranchLength = Vector3.Magnitude(currentBranchEnd - currentBranchStart);
        float deltaRatio = speedValue * Time.deltaTime / currentBranchLength;
        currentBranchRatio += deltaRatio;
        
        transform.position = PositionOnBranch(currentBranchRatio, currentBranchStart, currentBranchEnd, radius, angle);
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

    public float GetRadius()
    {
        return 3f;
    }
}
