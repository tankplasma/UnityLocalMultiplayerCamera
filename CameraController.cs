using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class CameraController : MonoBehaviour
{

    [SerializeField]
    Transform MaxVueCornerA, MaxVueCornerB , cameraTransform;

    [SerializeField]
    Transform[] CamFollowObjects; 

    Vector3 cameraAnchor;

    Vector3 globalSquare = Vector3.zero;
    Vector3 IntersectionPoint = Vector3.zero;

    float maxLerpValue;

    [SerializeField]
    float maxHeight, minHeight;


    [Range(1f, 100f)]
    [SerializeField]
    float deltaValue ;

    [SerializeField]
    bool activateSmoothFollow, drawInScenVue , followOnStart = true;

    [Range(0f, 1f)]
    [SerializeField]
    float smoothAmount;

    Coroutine controlCamera;

    private void Start()
    {
        maxLerpValue = (MaxVueCornerA.position - MaxVueCornerB.position).magnitude;

        if (followOnStart)
        {
            StartControl();
        }
    }

    public void StartControl()
    {
       controlCamera = StartCoroutine(ProcessControl());
    }

    public void StopControl()
    {
        StopCoroutine(controlCamera);
    }

    IEnumerator ProcessControl()
    {
        while (true)
        {
            if (CamFollowObjects.Length > 0)
            {
                GetCentralPos(CamFollowObjects);

                CameraFollow();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    void GetCentralPos(Transform[] allPlayerPos)
    {
        Vector2 Xpos = new Vector2(allPlayerPos[0].position.x , allPlayerPos[0].position.x); // -X , X
        Vector2 Ypos = new Vector2(allPlayerPos[0].position.z, allPlayerPos[0].position.z); // -Y , Y


        foreach (Transform pos in allPlayerPos)
        {
            Vector2 playerPos2D = new Vector2(pos.position.x,pos.position.z);

            if (playerPos2D.x < Xpos.x) 
                Xpos.x = playerPos2D.x;

            if (playerPos2D.x > Xpos.y)
                Xpos.y = playerPos2D.x;

            if (playerPos2D.y < Ypos.x)
                Ypos.x = playerPos2D.y;

            if (playerPos2D.y > Ypos.y)
                Ypos.y = playerPos2D.y;
        }

        Xpos += new Vector2(-deltaValue , deltaValue);
        Ypos += new Vector2(-deltaValue, deltaValue);

        Vector2 A1 = new Vector2(Xpos.x, Ypos.x);
        Vector2 A2 = new Vector2(Xpos.y, Ypos.y);
        Vector2 B1 = new Vector2(Xpos.x, Ypos.y);
        Vector2 B2 = new Vector2(Xpos.y, Ypos.x);

        float Compare = (A1 - A2).magnitude;

        float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);

        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

        IntersectionPoint =  new Vector3(
            B1.x + (B2.x - B1.x) * mu,
            0,
            B1.y + (B2.y - B1.y) * mu);

        cameraAnchor = IntersectionPoint;

        globalSquare = new Vector3(Xpos.y + Mathf.Abs(Xpos.x),
            allPlayerPos[0].position.y,
            (Ypos.y + Mathf.Abs(Ypos.x)));

        float LerpValue = Compare / maxLerpValue;

        Vector3 maxElevation = new Vector3(IntersectionPoint.x, maxHeight , IntersectionPoint.z);
        Vector3 MinElevation = new Vector3(IntersectionPoint.x, minHeight , IntersectionPoint.z);

        cameraAnchor = Vector3.Lerp(maxElevation,  MinElevation, 1 - LerpValue);
    }

    void CameraFollow()
    {
        Transform transformToFollow;

        if (cameraTransform)
            transformToFollow = cameraTransform;

        else
            transformToFollow = transform;

        if (activateSmoothFollow)
        {
            transformToFollow.position = Vector3.Lerp(transformToFollow.position, cameraAnchor, smoothAmount);
        }
        else
        {
            transformToFollow.position = cameraAnchor;
        }
    }

    private void OnDrawGizmos()
    {
        if (drawInScenVue)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireCube(IntersectionPoint, globalSquare);
        }
    }
}
