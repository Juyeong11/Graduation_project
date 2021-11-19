using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SANDBOXSoundTestProj : MonoBehaviour
{
    public AudioMixer masterMixer;
    public float sin;
    bool collapse = false;
    // Start is called before the first frame update
    void Start()
    {
        sin = 0.1f;
    }

    public void SetSin()
    {
        masterMixer.SetFloat("SinDelta", sin);
    }

    // Update is called once per frame
    void Update()
    {
        if (!collapse)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                sin += 0.01f;
                SetSin();
                collapse = true;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                sin -= 0.01f;
                SetSin();
                collapse = true;
            }
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            collapse = false;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            collapse = false;
        }
    }
}
