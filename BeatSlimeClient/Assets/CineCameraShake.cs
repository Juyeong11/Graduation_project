using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CineCameraShake : MonoBehaviour
{
    public static CineCameraShake instance {get; private set;}
    private CinemachineVirtualCamera cam;
    private CinemachineBasicMultiChannelPerlin noise;
    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingInstensity;

    public float intensityFactor;
    public float timeFactor;

    private void Awake() {
        instance = this;
        cam = GetComponent<CinemachineVirtualCamera>();
        noise = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 0;
        
    }

    public void ShakeCamera(float intensity, float time)
    {

        noise.m_AmplitudeGain = intensity;
        
        startingInstensity = intensity;
        shakeTimerTotal = time;
        shakeTimer = time;
    }

    public void ShakeCamera(float damage)
    {
        float intensity = damage * intensityFactor;
        float time = intensity * timeFactor;

        if (intensity > 3f)
            intensity = 3f;
        if (time > 0.5f)
            time = 0.5f;

        noise.m_AmplitudeGain = intensity;
        
        startingInstensity = intensity;
        shakeTimerTotal = time;
        shakeTimer = time;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("shake : " +shakeTimer);
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            noise.m_AmplitudeGain = Mathf.Lerp(startingInstensity, 0f, 1 - (shakeTimer / shakeTimerTotal)); 
         }

    }
}
