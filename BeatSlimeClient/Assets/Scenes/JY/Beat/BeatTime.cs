using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BeatTime
{
    public static BeatManager beatManager { get; set; }

    public static float SecPerBeat { get { return beatManager.SecPerBeat; } }
    public static float BeatSinceLevelLoad { get { return beatManager.BeatSinceLevelLoad; } }
    public static int BeatSinceLevelLoadInt { get { return beatManager.BeatSinceLevelLoadInt; } }
    public static float DeltaBeat { get { return beatManager.DeltaBeat; } }
    public static int CurBeat { get { return beatManager.CurBeat; } }
    public static float CurUpbeat { get { return beatManager.CurUpbeat; }}
    public static float ElapsedTimeToBeat(float time) => beatManager.ElapsedTimeToBeat(time);
    public static float BeatToElapsedTime(float beat) => beatManager.BeatToElapsedTime(beat);
    public static float TimeToBeat(float time) => beatManager.TimeToBeat(time);
    public static float BeatToTime(float beat) => beatManager.BeatToTime(beat);
    public static bool IsActiveBeat(int beat) => beatManager.IsActiveBeat(beat);

}
