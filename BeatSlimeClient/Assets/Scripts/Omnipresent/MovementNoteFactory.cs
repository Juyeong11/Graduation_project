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

    List<float> moveNotesBeats;

    float bpm;
    float timeByBeat;
    public float scrollSpeed;
    Vector3 translator;
    Vector3 Zero;

    private void Start()
    {
        leftNote = new List<RectTransform>();
        rightNote = new List<RectTransform>();
        moveNotesBeats = new List<float>();

        bpm = GameManager.data.bpm;
        timeByBeat = GameManager.data.timeByBeat;

        float tmp = timeByBeat;
        while (tmp <= BeatTime.beatManager.musicTime)
        {
            moveNotesBeats.Add(tmp);
            tmp += timeByBeat;
        }

        if (LeftNote.Count != RightNote.Count)
            Debug.LogError("left note != right note");

        translator = new Vector3(scrollSpeed, 0, 0);
        Zero = new Vector3(0, 50f, 0);

        for (int i=0;i<LeftNote.Count;++i)
        {
            leftNote.Add(LeftNote[i].GetComponent<RectTransform>());
            rightNote.Add(RightNote[i].GetComponent<RectTransform>());

            leftNote[i].anchoredPosition = -translator * (i+1) + Zero;
            rightNote[i].anchoredPosition = translator * (i+1) + Zero;
        }

    }


    void Update()
    {
        if (GameManager.data.isGameStart)
        {
            if (moveNotesBeats.Count > 0)
            {
                float beatPercentage = GameManager.data.nowSongTime;
                if (beatPercentage >= moveNotesBeats[0])
                {
                    moveNotesBeats.RemoveAt(0);
                }
                for (int i = 0; i < leftNote.Count; ++i)
                {
                    leftNote[i].anchoredPosition = -translator * (moveNotesBeats[i] - beatPercentage) + Zero;
                    rightNote[i].anchoredPosition = translator * (moveNotesBeats[i] - beatPercentage) + Zero;

                    if (leftNote[i].anchoredPosition.x >= 0)
                        leftNote[i].anchoredPosition = Zero;

                    if (rightNote[i].anchoredPosition.x <= 0)
                        rightNote[i].anchoredPosition = Zero;
                }
            }
        }
    }
}
