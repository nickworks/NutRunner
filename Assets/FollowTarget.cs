using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour {

    public Transform target;
	
	// Update is called once per frame
	void LateUpdate () {
        if (target)
        {
            transform.position = Vector3.Lerp(transform.position, target.position, .1f);
            transform.localRotation = Quaternion.Slerp(transform.rotation, target.rotation, .1f);
        }
	}
}
