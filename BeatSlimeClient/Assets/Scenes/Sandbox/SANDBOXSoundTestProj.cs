using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SANDBOXSoundTestProj : MonoBehaviour
{
    public AudioMixer masterMixer;
    public float sin;
    public float speeds;
    bool collapse = false;
    // Start is called before the first frame update
    void Start()
    {
        sin = 0.1f;
        speeds = 0.5f;
    }

    public void SetSin()
    {
        masterMixer.SetFloat("SinDelta", sin);
        masterMixer.SetFloat("Blend", speeds);
    }

    // Update is called once per frame
    void Update()
    {
        if (!collapse)
        {
            sin -= 0.1f;
            SetSin();

            if (sin < 0f)
                collapse = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            collapse = false;
            sin = speeds + 0.3f;
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            speeds += 0.1f;
            if (speeds > 1f)
                speeds = 1f;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            speeds -= 0.1f;
            if (speeds < 0f)
                speeds = 0f;
        }
    }
}
