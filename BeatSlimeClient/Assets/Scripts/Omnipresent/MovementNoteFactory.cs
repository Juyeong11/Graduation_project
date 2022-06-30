using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementNoteFactory : MonoBehaviour
{
    //object pool
    public List<GameObject> LeftNote;
    public List<GameObject> RightNote;
    List<RectTransform> leftNote;
    List<RectTransform> rightNote;

    public List<GameObject> LeftANote;
    public List<GameObject> RightANote;
    List<RectTransform> leftANote;
    List<RectTransform> rightANote;
    int ANoteIndex = 0;

    public float moveStartTime;
    bool movingStarted;

    List<float> moveNotesBeats;
    public List<float> attackNotesBeats = new List<float>();

    float bpm;
    float timeByBeat;
    public float scrollSpeed;
    Vector3 translator;
    Vector3 Zero;

    private void Start()
    {
        moveStartTime = 0;
        movingStarted = false;

        leftNote = new List<RectTransform>();
        rightNote = new List<RectTransform>();
        leftANote = new List<RectTransform>();
        rightANote = new List<RectTransform>();

        moveNotesBeats = new List<float>();

        bpm = GameManager.data.bpm;
        timeByBeat = GameManager.data.timeByBeat;

        float tmp = timeByBeat;
        while (tmp <= GameManager.data.totalSongTime)
        {
            moveNotesBeats.Add(tmp);
            tmp += timeByBeat;
        }

        if (LeftNote.Count != RightNote.Count)
            Debug.LogError("left note != right note");

        translator = new Vector3(scrollSpeed, 0, 0);
        Zero = new Vector3(0, 50f, 0);


        //초기 위치
        for (int i = 0; i < LeftNote.Count; ++i)
        {
            leftNote.Add(LeftNote[i].GetComponent<RectTransform>());
            rightNote.Add(RightNote[i].GetComponent<RectTransform>());

            leftNote[i].anchoredPosition = -translator * (i + 1) + Zero;
            rightNote[i].anchoredPosition = translator * (i + 1) + Zero;
            leftNote[i].gameObject.SetActive(false);
            rightNote[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < LeftANote.Count; ++i)
        {
            leftANote.Add(LeftANote[i].GetComponent<RectTransform>());
            rightANote.Add(RightANote[i].GetComponent<RectTransform>());

            leftANote[i].anchoredPosition = new Vector3(-1000f, 0f);
            rightANote[i].anchoredPosition = new Vector3(1000f, 0f);
        }

    }

    public void scrollUpdate()
    {
        translator = new Vector3(scrollSpeed, 0, 0);
    }


    void Update()
    {
        if (GameManager.data.isGameStart)
        {
            float beatPercentage = GameManager.data.nowSongTime;

            if (beatPercentage < GameManager.data.totalSongTime)
            {
                //Debug.Log(GameManager.data.nowSongTime);
                //Debug.Log(moveNotesBeats.Count);
                //Debug.Log(attackNotesBeats.Count);
                
                if (moveNotesBeats.Count > 0)
                {
                    for (int i = 0; i < leftNote.Count; ++i)
                    {
                        leftNote[i].anchoredPosition = -translator * (moveNotesBeats[i] + GameManager.data.MusicStartOffset - beatPercentage) + Zero;
                        rightNote[i].anchoredPosition = translator * (moveNotesBeats[i] + GameManager.data.MusicStartOffset - beatPercentage) + Zero;

                        if (leftNote[i].anchoredPosition.x >= 0)
                            leftNote[i].anchoredPosition = new Vector3(-1000f, 0f);

                        if (rightNote[i].anchoredPosition.x <= 0)
                            rightNote[i].anchoredPosition = new Vector3(-1000f, 0f);
                        
                        if (!movingStarted)
                        {
                            if (moveNotesBeats[i] >= moveStartTime)
                            {
                                leftNote[i].gameObject.SetActive(true);
                                rightNote[i].gameObject.SetActive(true);
                            }
                            if (beatPercentage > moveStartTime)
                                movingStarted = true;
                        }
                    }

                    if (beatPercentage >= moveNotesBeats[0] + GameManager.data.JudgementTiming + GameManager.data.MusicStartOffset)
                    {
                        moveNotesBeats.RemoveAt(0);
                        if (moveNotesBeats.Count < leftNote.Count)
                        {
                            leftNote.RemoveAt(leftNote.Count - 1);
                            rightNote.RemoveAt(leftNote.Count - 1);
                        }
                    }

                }

                if (attackNotesBeats.Count > 0)
                {
                    for (int i = 0; i < attackNotesBeats.Count; ++i)
                    {
                        //풀링
                        if (attackNotesBeats[i] - beatPercentage < 3000f)
                        {
                            leftANote[(ANoteIndex + i) % leftANote.Count].anchoredPosition = -translator * (attackNotesBeats[i] + GameManager.data.MusicStartOffset - beatPercentage) + Zero;
                            rightANote[(ANoteIndex + i) % rightANote.Count].anchoredPosition = translator * (attackNotesBeats[i] + GameManager.data.MusicStartOffset - beatPercentage) + Zero;

                            if (leftANote[(ANoteIndex + i) % leftANote.Count].anchoredPosition.x >= 0)
                                leftANote[(ANoteIndex + i) % leftANote.Count].anchoredPosition = new Vector3(-1000f, 0f);

                            if (rightANote[(ANoteIndex + i) % rightANote.Count].anchoredPosition.x <= 0)
                                rightANote[(ANoteIndex + i) % rightANote.Count].anchoredPosition = new Vector3(1000f, 0f);
                        }
                        else
                            break;

                    }

                    while (beatPercentage >= attackNotesBeats[0] + GameManager.data.JudgementTiming + GameManager.data.MusicStartOffset)
                    {
                        attackNotesBeats.RemoveAt(0);
                        ANoteIndex++;
                        if (attackNotesBeats.Count <= 0)
                            break;
                    }
                }
            }
        }
    }

    public bool parryCheck()
    {
        float beatPercentage = GameManager.data.nowSongTime;
        float distance = float.MaxValue;
        int finder = -1;

        for (int i = 0; i < attackNotesBeats.Count; i++)
        {
            if (attackNotesBeats[i] - beatPercentage > GameManager.data.JudgementTiming)
                break;

            if (distance > Mathf.Abs(attackNotesBeats[i] - beatPercentage))
            {
                distance = Mathf.Abs(attackNotesBeats[i] - beatPercentage);
                finder = i;
            }

        }

        for (int i=0;i<=finder;++i)
        {
            leftANote[(ANoteIndex) % leftANote.Count].anchoredPosition = new Vector3(-1000f, 0f);
            rightANote[(ANoteIndex) % rightANote.Count].anchoredPosition = new Vector3(1000f, 0f);

            attackNotesBeats.RemoveAt(0);
            ANoteIndex++;
        }

        if (finder != -1)
            return true;

        return false;
    }
}
