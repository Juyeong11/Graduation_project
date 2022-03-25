using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spriteBlinkMacro : MonoBehaviour
{
    public SpriteRenderer self;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if (self.enabled)
        {
            self.color = new Color(1,1,1,Mathf.Cos(Time.time * 3.0f)*0.4f + 0.7f);
        }
    }
}
