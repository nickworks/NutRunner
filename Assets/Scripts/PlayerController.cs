using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {


    public float speedForwards = 4;
    public float speedSideways = 2;

    bool inputReady = true;
    Branch activeChunk;

    #region Position Info
    int segment = 0;
    float segmentPercent = 0;
    int lane = 0;
    float lanePercent = 0;
    #endregion


    void Update()
    {

        if (activeChunk)
        {
            MoveSideways();
            RunForward();
            SetPositionAndRotation();
        }
        else
        {
            if (BranchManager.main.chunks.Count > 0) activeChunk = BranchManager.main.chunks[0];
        }
    }

    private void MoveSideways()
    {
        float axisH = Input.GetAxisRaw("Horizontal");

        lanePercent -= axisH * speedSideways * Time.deltaTime;
        if(lanePercent < 0)
        {
            lanePercent++;
            lane--;
        }
        if(lanePercent > 1)
        {
            lanePercent--;
            lane++;
        }

        if (lane < 0) lane = BranchMesh.SIDES - 1;
        if (lane > BranchMesh.SIDES - 1) lane = 0;
    }

    private void RunForward()
    {
        segmentPercent += speedForwards * Time.deltaTime;
        if (segmentPercent >= 1)
        {
            segmentPercent--;
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
        BranchMesh.BranchSurfaceProperties props =
            activeChunk.mesh.GetSurfaceProperties(segment, segmentPercent, lane, lanePercent);

        transform.position = props.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(props.forward, props.up), .1f);
    }
}
