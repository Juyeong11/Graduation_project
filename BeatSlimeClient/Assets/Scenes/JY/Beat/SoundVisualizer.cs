using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundVisualizer : MonoBehaviour
{
    private const int NUM_SAMPLES = 128;
    private const int HALF_SAMPLES = NUM_SAMPLES / 2;

    public float width = 300f;
    public float max = 10f;
    public float attenuation = 1f;
    public float pow = 1.5f;
    public float lowCut = 0.1f;
    public float highCut = 0.6f;
    public float updateTime = 0.05f;

    AudioSource audioSource;
    LineRenderer line;

    float[] spectrum = new float[NUM_SAMPLES];
    float[] coef = new float[NUM_SAMPLES];

    Vector3[] lineVertPositions;
    private int lineVertNum;

    private void Start()
    {
        audioSource = GameObject.FindObjectOfType<AudioSource>();
        line = GetComponent<LineRenderer>();

        lineVertNum = (int) ((highCut - lowCut) * NUM_SAMPLES);
        lineVertPositions = new Vector3[lineVertNum + 2];
        line.positionCount = lineVertNum;

        SetCoefArray();

        StartCoroutine(UpdateVisualizer());
    }

    IEnumerator UpdateVisualizer()
    {
        while(true)
        {
            audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

            int half = lineVertNum / 2;

            float maxSpectrum = 0.001f;
            for (int i = 0; i < lineVertNum; i++)
            {
                int index = (int)(NUM_SAMPLES * lowCut) + i;
                maxSpectrum = Mathf.Max(maxSpectrum, spectrum[index] * coef[index]);
            }


            for (int i = 0; i < lineVertNum; i++)
            {
                int sign = ((i & 1) == 0) ? 1 : -1;
                int index = (int)(NUM_SAMPLES * lowCut) + i;
                float x = (float)(i - half) / half * width;
                float y = spectrum[index] * coef[index] / maxSpectrum;
                y = Mathf.Pow(y, pow) * max;

                float prevY = lineVertPositions[i].y * sign;
                Vector3 result;
                if (prevY > y)
                    result = new Vector3(x, sign * prevY * attenuation);
                else
                    result = new Vector3(x, sign * y, 0f);
                lineVertPositions[i] = result;
            }
            line.SetPositions(lineVertPositions);

            yield return new WaitForSeconds(updateTime);
        }
    }

    private void SetCoefArray()
    {
        for(int i = 0; i < HALF_SAMPLES; i++)
        {
            coef[i] = Mathf.Pow((float) i / HALF_SAMPLES, 1f);
        }
        for(int i = HALF_SAMPLES; i < NUM_SAMPLES; i++)
        {
            coef[i] = 1f;
        }
    }
}
