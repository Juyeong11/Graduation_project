using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VFXManager : MonoBehaviour
{
    public static VFXManager data;

    public AudioMixer masterMixer;
    public float sin;
    public float speeds;
    //float Vv = 1.5f;
    bool collapse = false;
    // Start is called before the first frame update
    void Awake()
    {
        data = this;
    }
    void Start()
    {
        sin = 0.0f;
        speeds = 0.5f;
    }

    public void SetSin()
    {
        masterMixer.SetFloat("SinDelta", sin);
        masterMixer.SetFloat("Blend", speeds);
        //masterMixer.SetFloat("V", V);
    }

    // Update is called once per frame
    void Update()
    {
        if (!collapse)
        {
            sin -= 0.1f;
            SetSin();

            if (sin < 0f)
            {
                collapse = true;
                //masterMixer.SetFloat("V", 0f);
            }
        }

    }

    public void HitSounder(float b)
    {
        collapse = false;
        sin = b;
        //masterMixer.SetFloat("V", Vv);
    }
}
