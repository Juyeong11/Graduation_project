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
            lerped += 0.15f * t;
            //Debug.Log("L : " + lerped);
            foreach(var m in body.materials)
            {
                m.SetFloat("_Dissolve", lerped);
            }
            yield return new WaitForSeconds(0.1f);
        }
        Destroy(this.gameObject);
    }

    IEnumerator unDissolv(float t)
    {
        lerped = 1.5f;
        while (lerped > -1.5f)
        {
            lerped -= 0.1f * t;
            //Debug.Log("l : " + lerped); 
            foreach(var m in body.materials)
            {
                m.SetFloat("_Dissolve", lerped);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
