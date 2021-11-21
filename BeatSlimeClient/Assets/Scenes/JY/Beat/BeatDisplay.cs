using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatDisplay : MonoBehaviour
{
    public GameObject beatBar;
    public GameObject beatBarCenter;
    public GameObject upbeatBar;

    public float distance;
    public FloatVariable warningTime;

    public float centerBounceTime = 1f;
    public float centerBounceScale = 1.1f;
    private GameObject center;

    private int generatedBeats = 0;

    void Start()
    {
        center = Instantiate(beatBarCenter, this.transform);
        center.transform.localPosition = Vector3.zero;
    }

    void Update()
    {
        float generateTime = warningTime.Value + Time.timeSinceLevelLoad;
        float generateBeat = BeatTime.ElapsedTimeToBeat(generateTime);
        int generateBeatInt = Mathf.FloorToInt(generateBeat);

        if (generatedBeats < generateBeatInt)
        {
            generatedBeats++;

            if(BeatTime.IsActiveBeat(generateBeatInt))
            {
                GenerateBar(beatBar, distance, warningTime.Value);
            }
        }
        else return;
    }

    public void OnMusicBeat()
    {
        StartCoroutine(CenterBounce());
    }

    public void GenerateUpbeatBar()
    {
        float upbeatWarningTime = BeatTime.BeatToTime(BeatTime.CurBeat + 0.5f - BeatTime.BeatSinceLevelLoad);
        float upbeatDistance = distance / warningTime.Value * upbeatWarningTime;

        GenerateBar(upbeatBar, upbeatDistance, upbeatWarningTime);
    }

    private void GenerateBar(GameObject bar, float distance, float warningTime)
    {
        GameObject beatBar1 = Instantiate(bar, this.transform);
        GameObject beatBar2 = Instantiate(bar, this.transform);
        beatBar1.transform.localPosition = new Vector3(-distance, 0f, 0f);
        beatBar2.transform.localPosition = new Vector3(distance, 0f, 0f);

        beatBar1.SendMessage("MoveToOriginLocal", warningTime);
        beatBar2.SendMessage("MoveToOriginLocal", warningTime);
    }

    IEnumerator CenterBounce()
    {
        float timePassed = 0f;
        Vector3 initialScale = center.transform.localScale;

        while(timePassed < centerBounceTime / 2)
        {
            float scale = Mathf.SmoothStep(1f, centerBounceScale, timePassed * 2 / centerBounceTime);
            center.transform.localScale = initialScale * scale;
            timePassed += Time.deltaTime;
            yield return null;
        }

        while(timePassed < centerBounceTime)
        {
            float scale = Mathf.SmoothStep(centerBounceScale, 1f, (timePassed - centerBounceTime / 2) * 2 / centerBounceTime);
            center.transform.localScale = initialScale * scale;
            timePassed += Time.deltaTime;
            yield return null;
        }

        center.transform.localScale = initialScale;
    }
}
