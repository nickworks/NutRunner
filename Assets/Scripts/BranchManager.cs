using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchManager : MonoBehaviour {

    public static BranchManager main;

    public List<Branch> chunks = new List<Branch>();
    public Branch prefabBranch;
    public int randomSeed = 0;

    bool runUnloadTimer = false;
    float unloadTimer = 0;
	
    void Start()
    {
        main = this;
        Random.InitState(randomSeed);
    }
	void Update ()
    {
        if (runUnloadTimer) UnloadTimer();

        while (chunks.Count < 4)
        {
            SpawnNextChunk();
        }
    }

    private void UnloadTimer()
    {
        unloadTimer -= Time.deltaTime;
        if (unloadTimer <= 0)
        {
            UnloadChunk();
            runUnloadTimer = false;
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
    public Branch fetchNext(Branch branch)
    {
        runUnloadTimer = true;
        unloadTimer = 1;
        int index = chunks.IndexOf(branch) + 1;
        if (index >= chunks.Count) return null;
        return chunks[index];
    }
}
