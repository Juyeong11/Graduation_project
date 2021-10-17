using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pattern
{
    public static int id_ = 0;
    public int id;
    public Beat rhythmBeat;
    public Beat warningBeatOffset;
    public int noteType;
    public string pivotType;
    public HexCoordinates pivot;
    public HexDirection direction;

    public Pattern(){}

    public Pattern(Dictionary<string, object> datas, int prebar)
    {
        id_++;
        id = id_;
        rhythmBeat = new Beat();
        warningBeatOffset = new Beat();

        noteType = int.Parse(datas["noteType"].ToString());
        pivotType = datas["pivotType"].ToString();

        if (int.Parse(datas["pivotX"].ToString()) + int.Parse(datas["pivotY"].ToString()) + int.Parse(datas["pivotZ"].ToString()) != 0)
        {
            Debug.LogError("PatternPivotLoad : None-ZeroSum Error!!");
        }
        pivot.setCoordinates(int.Parse(datas["pivotX"].ToString()), int.Parse(datas["pivotZ"].ToString()));

        rhythmBeat.Set(prebar,
                                int.Parse(datas["4th"].ToString()),

                                int.Parse(datas["3rd"].ToString()) * 8 + int.Parse(datas["6th"].ToString()) * 4 +
                                int.Parse(datas["12th"].ToString()) * 2 + int.Parse(datas["24th"].ToString()),

                                int.Parse(datas["8th"].ToString()) * 2 + int.Parse(datas["16th"].ToString()));

        switch(datas["speed"].ToString())
        {
            case "0": // default : 2beat
                warningBeatOffset.Set(0, 2);
                break;
            case "1":
                //8beat
                warningBeatOffset.Set(0,0,1);
                break;
            case "2":
                //4beat
                warningBeatOffset.Set(0, 1);
                break;
            case "3":
                //2beat
                warningBeatOffset.Set(0, 2);
                break;
            case "4":
                //1bar
                warningBeatOffset.Set(1);
                break;
            default:
                Debug.LogError("PatternSpeedLoad : Breaking Number Error!!");
                break;
        }

        switch (datas["direction"].ToString())
        {
            case "LU":
                direction = HexDirection.LeftUp;
                break;
            case "U":
                direction = HexDirection.Up;
                break;
            case "RU":
                direction = HexDirection.RightUp;
                break;
            case "LD":
                direction = HexDirection.LeftDown;
                break;
            case "D":
                direction = HexDirection.Down;
                break;
            case "RD":
                direction = HexDirection.RightDown;
                break;
            default:
                direction = HexDirection.Up;
                break;
        }
     }

    public Beat GetAppearBeat()
    {
        return rhythmBeat - warningBeatOffset;
    }

    
}
