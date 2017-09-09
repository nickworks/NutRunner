using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchManager : MonoBehaviour {

    List<Branch> chunks = new List<Branch>();
    public Branch prefabBranch;

    float unloadTimer = 0;
	
	void Update () {

        unloadTimer -= Time.deltaTime;
        if(unloadTimer <= 0)
        {
            UnloadChunk();
            unloadTimer = 2;
        }

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
    void UnloadChunk()
    {
        if (chunks.Count > 0)
        {
            Destroy(chunks[0].gameObject);
            chunks.RemoveAt(0);
        }
    }
}
