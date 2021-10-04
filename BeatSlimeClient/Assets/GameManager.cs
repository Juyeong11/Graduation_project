using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager data;

    public PlayerManager player;

    public SoundManager soundManager;
    public SoundEffectManager soundEffectManager;
    public SoundProgressBar soundProgressBar;

    public bool isGameStart;

    public string SongName;

    public float bpm;
    public int barCounts = 4;
    public int totalSongTime;
    public int nowSongTime;
    public int timeByBar;
    public int timeByBeat;
    public int nowBars;
    public int nowBeats;
    public int beatCounter = 0;

    public int offsetTime;

    public int JudgementTiming;

    public int alreadyMoved;

    void Start()
    {
        data = this;
        isGameStart = false;
        alreadyMoved = 0;

        bpm = SoundManager.instance.GetBGMBpm(SongName);
        totalSongTime = (int)(SoundManager.instance.GetMusicLength(SongName) * 1000f);
        nowSongTime = 0;
        timeByBeat = (int)(1000f * 60f / bpm);
        timeByBar = timeByBeat * barCounts;
    }

    public void PlaySound()
    {
        if (!isGameStart)
        {
            soundManager.PlayBGM(SongName);
            soundEffectManager.BeatEffect();
            beatCounter = timeByBeat;
            isGameStart = true;
        }
    }

    void FixedUpdate()
    {
        if (isGameStart)
        {
            int prevBeats = nowBeats;

            nowSongTime = SoundManager.instance.GetMusicLapsedTime() - offsetTime;
            if (nowSongTime > 0)
            {
                nowBars = nowSongTime / timeByBar;
                nowBeats = (nowSongTime / timeByBeat) % barCounts;
                beatCounter -= (int)(Time.deltaTime * 1000f);

                if (prevBeats != nowBeats)
                {
                    soundEffectManager.BeatEffect();
                    beatCounter = timeByBeat;
                }
            }

            if (alreadyMoved > 0)
                alreadyMoved -= (int)(Time.deltaTime * 1000f);
        }
    }

    public int getIsGoodTiming()
    {
        if (alreadyMoved <= 0)
        {
            if (beatCounter <= JudgementTiming)
            {
                return -1;  //일찍 누름
            }
            else if (timeByBeat - beatCounter <= JudgementTiming)
            {
                return 1;   //늦게 누름
            }
            else
            {
                Debug.Log("Error Beside : -" + beatCounter + ", +" + (timeByBeat - beatCounter));
                return 0;   //잘못 누름
            }
        }
        else
        {
            Debug.Log("Cutting");
        }
        return 0;
    }

    public void setMoved()
    {
        if (beatCounter <= JudgementTiming)
        {
            Debug.Log("Judge : -" + beatCounter);
        }
        else if (timeByBeat - beatCounter <= JudgementTiming)
        {
            Debug.Log("Judge : +" + (timeByBeat - beatCounter));
        }

        alreadyMoved = JudgementTiming*2;
    }

}
