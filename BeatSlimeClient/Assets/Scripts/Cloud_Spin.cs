using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud_Spin : MonoBehaviour
{
    public float degree = 0.0f;
    // Start is called before the first frame update
    
    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(new Vector3(0, degree, 0));
    }
}
