using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MDissolveIlluminator : MonoBehaviour
{
    public MeshRenderer body;

    public float dissolveSpeed;

    float lerped;

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.F6))
    //     {
    //         PrintDissolve();
    //     }
    //     if (Input.GetKeyDown(KeyCode.F7))
    //     {
    //         PrintUnDissolve();
    //     }
    // }
    public void PrintDissolve()
    {
        Debug.Log("dStart");
        StartCoroutine(dissolv(dissolveSpeed));
    }

    public void PrintUnDissolve()
    {
        //Debug.Log("dEnd");
        StartCoroutine(unDissolv(dissolveSpeed));
    }

    IEnumerator dissolv(float t)
    {
        lerped = -1.5f;
        while (lerped < 1.5f)
        {
            lerped += Time.deltaTime * t;
            foreach(var m in body.materials)
            {
                m.SetFloat("_Dissolve", lerped);
            }
            yield return null;
        }
        Destroy(this.gameObject);
    }

    IEnumerator unDissolv(float t)
    {
        lerped = 1.5f;
        while (lerped > -1.5f)
        {
            lerped -= Time.deltaTime * t;
            foreach(var m in body.materials)
            {
                m.SetFloat("_Dissolve", lerped);
            }
            yield return null;
        }
    }
}
