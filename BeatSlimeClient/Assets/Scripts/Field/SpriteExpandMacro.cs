using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteExpandMacro : MonoBehaviour
{
    public float speed;
    float factor = 0;
    void Update()
    {
        if (gameObject.activeSelf)
        {
            if (factor < 1.0f)
            {
                factor += Time.deltaTime * speed;
                gameObject.transform.localScale = new Vector3(factor,factor,factor);
            }
        }
    }
}
