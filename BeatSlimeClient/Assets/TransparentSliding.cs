using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentSliding : MonoBehaviour
{
    public RectTransform self;
    public Material mat;
    private float tr;

    void Start()
    {
        self.localScale = new Vector3(Screen.width / 1920f, 1, 1);
        //self.rect.Set(self.rect.x, self.rect.y, Screen.width/4f, self.rect.height);
        //print(self.rect.xMin + " " + self.rect.yMin + " " +  Screen.width/4f + " " + self.rect.height);
        //tr = mat.GetFloat("_TransparentPower");
    }
    
    void Update()
    {
        // tr = 0.3f*Mathf.Sin(Time.time) + 1.05f;
        // mat.SetFloat("_TransparentPower", tr);
    }
}
