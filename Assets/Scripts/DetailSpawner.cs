using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Branch))]
public class DetailSpawner : MonoBehaviour {

    public GameObject prefabBranch3;
    public GameObject prefabLichen;
    public GameObject prefabMoss;


    // Use this for initialization
    public void AddDetails()
    {

        PlaceOnSurface(prefabBranch3);
        PlaceOnSurface(prefabLichen);
        PlaceOnSurface(prefabMoss);

    }
    private void PlaceOnSurface(GameObject prefab)
    {
        Branch branch = GetComponent<Branch>();
        int n = (int)(Random.value * branch.finalPoints.Length);
        int l = (int)(Random.value * BranchMesh.SIDES);

        BranchMesh.BranchSurfaceProperties props =
            branch.mesh.GetSurfaceProperties(n, 0, l, 0);

        Instantiate(prefab, props.position, Quaternion.LookRotation(props.forward, props.up), transform);

    }
}
