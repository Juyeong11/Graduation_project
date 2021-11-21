using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatBar : MonoBehaviour
{
    public void MoveToOriginLocal(float time)
    {
        StartCoroutine(MoveLocal(transform.localPosition, Vector3.zero, time));

        Destroy(gameObject, time);
    }

    IEnumerator MoveLocal(Vector3 from, Vector3 to, float time)
    {
        float timePassed = -Time.deltaTime;
        Vector3 disposition = to - from;

        while(timePassed < time)
        {
            timePassed += Time.deltaTime;
            float ratio = timePassed / time;

            this.transform.localPosition = from + disposition * ratio;

            yield return null;
        }

        transform.position = to;
    }
}
