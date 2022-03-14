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
            //print("patternLoad : " + tmpP.id);
            //DEBUG : 이펙트 패킷은 서버에서 보내줄 필요 없이 여기서 바로 읽어서 저장해놓고 사용하기 (동접자 올릴 때)
            
            //자신에게 날아오는 보스의 유도 공격만!
            if (tmpP.noteType.ToString() == "10" && tmpP.pivotType == ("Player" + PID))
            {
                Debug.Log(tmpP.id + " " + tmpP.noteType + " " + tmpP.pivotType);
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
            //여기서 이펙트 출력하면 되겠군
            //

            HexCoordinates TargetPos = GameManager.data.GetTargetPos(pattern[0].pivotType);
            HexCoordinates BossPos = GameManager.data.GetBossPos();
            TargetPos.plus(pattern[0].pivot.X, pattern[0].pivot.Z);
            int charging_time = pattern[0].warningBeatOffset.GetBeatTime();
            switch (pattern[0].noteType)
            {
                case 3:
                case 4:
                    EffectManager.instance.BossTileEffect(TargetPos.X, TargetPos.Y, TargetPos.Z, charging_time, pattern[0].noteType);
                    break;

                case 99:
                    EffectManager.instance.OneTileEffect(TargetPos.X, TargetPos.Y, TargetPos.Z, charging_time);
                    break;

                case 5:
                    EffectManager.instance.BossWaterGunEffect(BossPos.getRealPosition(), TargetPos.getRealPosition(), charging_time);

                    break;

                case 6:
                    EffectManager.instance.BossQuakeEffect(TargetPos.X, TargetPos.Y, TargetPos.Z, charging_time, GameManager.data.GetBossDir());

                    break;

                case 10:
                    EffectManager.instance.BossTargetingEffect(BossPos.getRealPosition(), ref GameManager.data.GetPlayerREF(pattern[0].pivotType), charging_time);

                    break;

            }
            

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
