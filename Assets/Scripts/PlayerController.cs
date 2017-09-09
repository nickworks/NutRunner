using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {


    public float speed = 4;

    Branch activeChunk;
    int segment = 0;
    int lane = 0;
    float percent = 0;
    bool inputReady = true;

    void Start()
    {
        
    }
    void Update()
    {

        if (activeChunk)
        {
            float axisH = Input.GetAxisRaw("Horizontal");
            if (axisH == 0)
            {
                inputReady = true;
            }
            else
            {
                if(inputReady) lane -= (int)axisH;
                inputReady = false;
            }
            if (lane < 0) lane = BranchMesh.SIDES - 1;
            if (lane > BranchMesh.SIDES - 1) lane = 0;

            RunForward();
            SetPositionAndRotation();
        }
        else
        {
            if (BranchManager.main.chunks.Count > 0) activeChunk = BranchManager.main.chunks[0];
        }
    }

    private void RunForward()
    {
        percent += speed * Time.deltaTime;
        if (percent >= 1)
        {
            percent--;
            segment++;
            if (segment >= activeChunk.finalPoints.Length - 1)
            {
                segment = 0;
                activeChunk = BranchManager.main.fetchNext(activeChunk);
            }
        }
    }

    private void SetPositionAndRotation()
    {
        BranchMesh.BranchSurfaceProperties props = activeChunk.mesh.GetSurfaceProperties(segment, lane, percent);

        transform.position = props.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(props.forward, props.up), .1f);
    }
}
