using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PatternManager : MonoBehaviour
{
    public static PatternManager data;
    //public UnityEvent SetSkill;
    //public UnityEvent CastSkill;
    public List<Pattern> pattern;
    public List<Pattern> settedPattern;

    public Pattern SettedPattern;
    public Pattern CastedPattern;

    public MovementNoteFactory Factory;

    public Image LOGO;

    private void Awake()
    {
        pattern = new List<Pattern>();
        settedPattern = new List<Pattern>();
        data = this;
        LOGO.gameObject.SetActive(false);
        LOGO.fillAmount = 0;
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
                //Debug.Log(tmpP.id + " " + tmpP.noteType + " " + tmpP.pivotType);
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
            //SetSkill.Invoke();
            //여기서 이펙트 출력하면 되겠군
            //

            HexCoordinates TargetPos = GameManager.data.GetTargetPos(pattern[0].pivotType);
            HexCoordinates BossPos = GameManager.data.GetBossPos();
            TargetPos.plus(pattern[0].pivot.X, pattern[0].pivot.Z);
            int charging_time = pattern[0].warningBeatOffset.GetBeatTime();
            switch (pattern[0].noteType)
            {
                case -1:
                    //이동만 이례적으로 애니메이션 바인드
                    GameManager.data.GetEnemyAnim().SetTrigger("Move");
                    //GameManager.data.enemy
                    break;

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
                    EffectManager.instance.BossQuakeEffect(TargetPos.X, TargetPos.Y, TargetPos.Z, charging_time, pattern[0].direction);

                    break;

                case 10:
                    EffectManager.instance.BossTargetingEffect(BossPos.getRealPosition(), ref GameManager.data.GetPlayerREF(pattern[0].pivotType), charging_time);

                    break;

                case 600:   //게임 시작 패널
                    StartCoroutine(LogoLoad(true));
                    break;
                case 601:   //게임 시작 패널
                    StartCoroutine(LogoLoad(false));
                    break;

                //보스 애니메이션
                case 1000:
                    GameManager.data.GetEnemyAnim().SetTrigger("Move");
                    break;
                case 1001:
                    GameManager.data.GetEnemyAnim().SetTrigger("Attack");
                    break;
                case 1002:
                    GameManager.data.GetEnemyAnim().SetTrigger("Attack2");
                    break;
                case 1003:
                    GameManager.data.GetEnemyAnim().SetTrigger("Attack3");
                    break;
                case 1004:
                    GameManager.data.GetEnemyAnim().SetTrigger("Attack4");
                    break;
            }
            //Debug.Log("patternType : " + pattern[0].noteType);

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
            //CastSkill.Invoke();
            settedPattern.RemoveAt(0);
            patNums++;
        }
        return patNums;
    }

    IEnumerator LogoLoad(bool open)
    {
        if (open)
        {
            LOGO.gameObject.SetActive(true);
            while (LOGO.fillAmount < 1)
            {
                LOGO.fillAmount += 0.01f;
                yield return new WaitForSeconds(0.01f);
            }
        }
        else
        {
            while (LOGO.fillAmount > 0)
            {
                LOGO.fillAmount -= 0.02f;
                yield return new WaitForSeconds(0.01f);
            }
            LOGO.gameObject.SetActive(false);
        }
    }
}
