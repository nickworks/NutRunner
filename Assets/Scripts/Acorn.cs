﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Acorn : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(new Vector3(0, 1, 0), 100f * Time.deltaTime);
	}

    private void OnDestroy()
    {
        //TODO: Disappearing particle flourish?
    }
}
