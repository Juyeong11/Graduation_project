using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PatternManager : MonoBehaviour
{
    public static PatternManager data;
    public UnityEvent SetSkill;
    public UnityEvent CastSkill;
    public List<Pattern> pattern;
    public List<Pattern> settedPattern;

    public Pattern SettedPattern;
    public Pattern CastedPattern;

    public MovementNoteFactory Factory;

    private void Awake()
    {
        pattern = new List<Pattern>();
        settedPattern = new List<Pattern>();
        data = this;
    }

    public void Load(int myType)
    {
        StartCoroutine(PatternLoad(myType+1));
    }


    IEnumerator PatternLoad(int PID)
    {
        List<Dictionary<string, object>> datas = CSVReader.Read("Pattern/" + GameManager.data.SongName);
        int preBar = 0;

        for (int i = 0; i < datas.Count; ++i)
        {
            ///PATTERN LOAD
            if (datas[i]["noteType"].ToString() == "0")
                continue;

            if (int.Parse(datas[i]["bar"].ToString()) != 0)
                preBar = int.Parse(datas[i]["bar"].ToString());

            Pattern tmpP = new Pattern(datas[i], preBar);
            pattern.Add(tmpP);

            //자신에게 날아오는 보스의 유도 공격만!
            if (tmpP.noteType == 10 && tmpP.pivotType == ("Player" + PID))
            {
                Factory.attackNotesBeats.Add(tmpP.rhythmBeat.GetBeatTime());
            }
            //DEBUG : 장판 공격도 작은 노트 보여주고싶음 (비트 알려주기 위해)
        }
        yield return null;
    }

    public int PeekPattern(Beat b)
    {
        int patNums = 0;
        while(pattern.Count > 0 && pattern[0].GetAppearBeat() <= b)
        {
            SettedPattern = pattern[0];
            SetSkill.Invoke();
            settedPattern.Add(pattern[0]);
            pattern.RemoveAt(0);
            patNums++;
        }
        return patNums;
    }

    public int ServePattern(Beat b)
    {
        int patNums = 0;
        while (settedPattern.Count > 0 && settedPattern[0].rhythmBeat <= b)
        {
            CastedPattern = settedPattern[0];
            //패턴 사용?
            CastSkill.Invoke();
            settedPattern.RemoveAt(0);
            patNums++;
        }
        return patNums;
    }
}
