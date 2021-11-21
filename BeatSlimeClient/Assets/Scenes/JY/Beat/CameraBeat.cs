using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBeat : MonoBehaviour
{
    public float bounceTime;
    public float bounceSize;

    private bool isBouncing = false;
    private float passedTime = 0f;

    public void Bounce()
    {
        if(!isBouncing)
            StartCoroutine(BounceCoroutine());
    }

    IEnumerator BounceCoroutine()
    {
        isBouncing = true;
        passedTime = 0f;
        Camera camera = Camera.main;
        float initialSize = camera.orthographicSize;

        while (passedTime < bounceTime)
        {
            passedTime += Time.deltaTime;

            camera.orthographicSize += Mathf.Cos(passedTime / bounceTime * Mathf.PI) * bounceSize;

            yield return null;
        }

        camera.orthographicSize = initialSize;
        isBouncing = false;
    }
}
