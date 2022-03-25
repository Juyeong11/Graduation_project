using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveFrustum : MonoBehaviour
{
    public GameObject selfMesh;
    void Update()
    {
        var plane = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        var point = transform.position;

        foreach(var p in plane)
        {
            if (p.GetDistanceToPoint(point) < -0.5f)
            {
                //Debug.Log("Out of Frustum");
                selfMesh.SetActive(false);
                return;
            }
        }
        selfMesh.SetActive(true);
    }
}
