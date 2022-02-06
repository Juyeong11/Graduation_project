using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    float t = 0;
    float i = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        i = Mathf.Lerp(0, 3.14f, t);
        Debug.Log(i);
        Debug.Log(Mathf.Sin(i));
    }
}
