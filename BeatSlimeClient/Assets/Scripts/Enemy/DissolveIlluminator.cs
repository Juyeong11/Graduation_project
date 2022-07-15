using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveIlluminator : MonoBehaviour
{
    public SkinnedMeshRenderer body;
    public SkinnedMeshRenderer head;

    public float dissolveSpeed;

    float lerped;
    public void PrintDissolve()
    {
        //Debug.Log("dStart");
        StartCoroutine(dissolv(dissolveSpeed));
    }

    public void PrintUnDissolve()
    {
        //Debug.Log("dEnd");
        StartCoroutine(unDissolv(dissolveSpeed));
    }

    IEnumerator dissolv(float t)
    {
        lerped = -2.5f;
        while (lerped < 1.5f)
        {
            lerped += Time.deltaTime * t;
            foreach(var m in body.materials)
            {
                m.SetFloat("_Dissolve", lerped);
            }
             foreach(var m in head.materials)
            {
                m.SetFloat("_Dissolve", lerped);
            }
            yield return null;
        }
    }

    IEnumerator unDissolv(float t)
    {
        lerped = 1.5f;
        while (lerped > -2.5f)
        {
            lerped -= Time.deltaTime * t;
            foreach(var m in body.materials)
            {
                m.SetFloat("_Dissolve", lerped);
            }
             foreach(var m in head.materials)
            {
                m.SetFloat("_Dissolve", lerped);
            }
            yield return null;
        }
    }
}
