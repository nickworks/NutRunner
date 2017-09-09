using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchManager : MonoBehaviour {

    List<Branch> chunks = new List<Branch>();
    public Branch prefabBranch;

    void Start () {
		
	}
	
	void Update () {
		while(chunks.Count < 4)
        {
            SpawnNextChunk();
        }
	}
    void SpawnNextChunk()
    {
        Branch parent = (chunks.Count > 0) ? chunks[chunks.Count - 1] : null; // get previous chunk

        Branch newBranch = Instantiate(prefabBranch);
        chunks.Add(newBranch);
        newBranch.Init(parent);
    }
}
