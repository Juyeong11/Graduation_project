using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentSliding : MonoBehaviour
{
    public Material mat;
    private float tr;

    void Start() {
        tr = mat.GetFloat("_TransparentPower");
    }
    
    void Update()
    {
        tr = 0.2f*Mathf.Sin(Time.time) + 1.2f;
        mat.SetFloat("_TransparentPower", tr);
    }
}
