using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beat
{
    int bar;
    public int addBeat = 0;
    int add24 = 0;
    int add32 = 0;

    public Beat()
    {
        bar = 0;
        addBeat = 0;
        add24 = 0;
        add32 = 0;
    }

    public Beat(int bars, int beats = 0, int triplet = 0, int quater = 0)
    {
        bar = bars;
        addBeat = beats;
        add24 = triplet;
        add32 = quater;
    }

    public void Set(int bars, int beats = 0, int triplet = 0, int quater = 0)
    {
        bar = bars;
        addBeat = beats;
        add24 = triplet;
        add32 = quater;
    }

    public int GetBeatTime()
    {
        return bar * GameManager.data.timeByBar
            + addBeat * GameManager.data.timeByBeat
            + add24 * GameManager.data.timeBy24Beat
            + add32 * GameManager.data.timeBy32Beat;
    }

    public static bool operator ==(Beat a,Beat b)
    {
        if (a.bar == b.bar &&
             a.addBeat == b.addBeat &&
             a.add24 == b.add24 &&
             a.add32 == b.add32)
        {
            return true;
        }
        else return false;
    }

    public static bool operator !=(Beat a, Beat b)
    {
        if (a == b)
            return false;
        else
            return true;
    }

    public static bool operator ==(Beat a, int time)
    {
        if (a.bar * GameManager.data.timeByBar
            + a.addBeat * GameManager.data.timeByBeat
            + a.add24 * GameManager.data.timeBy24Beat
            + a.add32 * GameManager.data.timeBy32Beat == time)
        {
            return true;
        }
        else return false;
    }

    public static bool operator !=(Beat a, int time)
    {
        if (a == time)
            return false;
        else
            return true;
    }

    public override bool Equals(object op1)
    {
        Debug.LogError("Please override Equals (of class 'Beat'), now value Ignored by false");
        return false;
    }

    //GetHashCode 는 개체의 상태에 기반한 고유한 int 값을 얻는데 사용 한다.
    public override int GetHashCode()
    {
        Debug.LogError("Please override GetHashCode (of class 'Beat'), now value Ignored by 0");
        return 0;
    }


}
