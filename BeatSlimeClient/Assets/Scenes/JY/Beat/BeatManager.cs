using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    public float BPM;
    public float musicTime;
    public float offsetTime;
    public int warningBeat;
    public GameEvent onMusicBeat;
    AudioSource audioSource;

    protected float secPerBeat;
    protected List<bool> ActiveBeatList = new List<bool>(600);
    protected float offsetTimeInternal;
    protected double dspOffsetTime;
    protected float OffsetTime => offsetTime + offsetTimeInternal;

    public virtual float SecPerBeat { get { return secPerBeat; } }
    public virtual float BeatSinceLevelLoad { get { return (Time.timeSinceLevelLoad + OffsetTime) / secPerBeat; } }
    public virtual int BeatSinceLevelLoadInt { get { return Mathf.FloorToInt(BeatSinceLevelLoad); } }
    public virtual float DeltaBeat { get { return Time.deltaTime / secPerBeat; } }
    public virtual int CurBeat { get { return Mathf.RoundToInt(BeatSinceLevelLoad); } }
    public virtual float CurUpbeat { get { return BeatSinceLevelLoadInt + 0.5f; }}

    public virtual float ElapsedTimeToBeat(float time) => (time + OffsetTime) / secPerBeat;
    public virtual float BeatToElapsedTime(float beat) => beat * secPerBeat - OffsetTime;
    public virtual float TimeToBeat(float time) => time / secPerBeat;
    public virtual float BeatToTime(float beat) => beat * secPerBeat;
    public virtual bool IsActiveBeat(int beat) => beat < 0 ? false : ActiveBeatList[beat];


    private void Awake()
    {
        BeatTime.beatManager = this;
        secPerBeat = 60f / BPM;
        SetActiveBeatList();
    }

    public virtual void MusicStart(AudioSource audio)
    {
        audioSource = audio;
        audioSource.PlayScheduled(AudioSettings.dspTime);

        dspOffsetTime = AudioSettings.dspTime - Time.timeSinceLevelLoad;
    }

    protected virtual void Update()
    {
        AdjustOffset();
        CheckForBeatEvent();
    }

    private void AdjustOffset()
    {
        offsetTimeInternal = (float) (AudioSettings.dspTime - Time.timeSinceLevelLoad - dspOffsetTime);
    }

    private int beatEventCount = 0;
    protected virtual void CheckForBeatEvent()
    {
        if(beatEventCount < BeatSinceLevelLoadInt)
        {
            beatEventCount++;

            if (IsActiveBeat(beatEventCount))
                onMusicBeat.Raise(null);
        }
    }

    protected virtual void SetActiveBeatList()
    {
        int beat;
        for (beat = 0; beat < warningBeat; beat++)
            ActiveBeatList.Add(false);
        while (BeatToElapsedTime(beat++) < musicTime)
            ActiveBeatList.Add(true);
    }
}
