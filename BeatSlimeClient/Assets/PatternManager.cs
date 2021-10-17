using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PatternManager : MonoBehaviour
{
    public static PatternManager data;
    public UnityEvent CastSkill;
    public List<Pattern> pattern;
    public List<Pattern> settedPattern;

    public Pattern CastedPattern;

    private void Awake()
    {
        pattern = new List<Pattern>();
        settedPattern = new List<Pattern>();
        data = this;
    }
    void Start()
    {
        StartCoroutine(PatternLoad());
    }


    IEnumerator PatternLoad()
    {
        List<Dictionary<string, object>> datas = CSVReader.Read("Pattern/" + GameManager.data.SongName);
        int preBar = 0;

        for (int i = 0; i < datas.Count; ++i)
        {
            if (datas[i]["noteType"].ToString() == "0")
                continue;

            if (int.Parse(datas[i]["bar"].ToString()) != 0)
                preBar = int.Parse(datas[i]["bar"].ToString());

            pattern.Add(new Pattern(datas[i], preBar));
        }
        yield return null;
    }

    public int PeekPattern(Beat b)
    {
        int patNums = 0;
        while(pattern.Count > 0 && pattern[0].GetAppearBeat() <= b)
        {
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
