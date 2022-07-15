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
    public List<GameObject> itemModels;

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

            if (datas[i]["noteType"].ToString() == "999")
            {
                BeatTime.beatManager.BPM = float.Parse(datas[i]["pivotType"].ToString());
                SoundManager.instance.SetBPM(datas[i]["pivotZ"].ToString(), int.Parse(datas[i]["pivotType"].ToString()));
                GameManager.data.JudgementTiming = int.Parse(datas[i]["pivotY"].ToString());
                PatternManager.data.Factory.scrollSpeed = float.Parse(datas[i]["pivotX"].ToString());

                GameManager.data.bpm = SoundManager.instance.GetBGMBpm(GameManager.data.SongName);
                GameManager.data.totalSongTime = (int)(SoundManager.instance.GetMusicLength(GameManager.data.SongName) * 1000f);
                GameManager.data.nowSongTime = 0;
                GameManager.data.timeByBeat = (int)(1000f * 60f / GameManager.data.bpm);
                GameManager.data.timeByBar = GameManager.data.timeByBeat * GameManager.data.barCounts;
                GameManager.data.timeBy24Beat = GameManager.data.timeByBeat / 6;
                GameManager.data.timeBy16Beat = GameManager.data.timeByBeat / 4;
                continue;
            }

            if (int.Parse(datas[i]["bar"].ToString()) != 0)
                preBar = int.Parse(datas[i]["bar"].ToString());

            Pattern tmpP = new Pattern(datas[i], preBar);
            pattern.Add(tmpP);
            //print("patternLoad : " + tmpP.id);
            //DEBUG : 이펙트 패킷은 서버에서 보내줄 필요 없이 여기서 바로 읽어서 저장해놓고 사용하기 (동접자 올릴 때)
            
            //자신에게 날아오는 보스의 유도 공격만!
            if (tmpP.noteType.ToString() == "10" && (tmpP.pivotType == ("Player" + PID) || tmpP.pivotType == ("PlayerA")))
            {
                //Debug.Log(tmpP.id + " " + tmpP.noteType + " " + tmpP.pivotType);
                Factory.attackNotesBeats.Add(tmpP.rhythmBeat.GetBeatTime());
            }
            if (tmpP.noteType.ToString() == "700")
            {
                Factory.moveStartTime = tmpP.rhythmBeat.GetBeatTime();
            }
            //DEBUG : 장판 공격도 작은 노트 보여주고싶음 (비트 알려주기 위해)
        }

        pattern.Sort((x, y) => x.GetAppearBeat().GetBeatTime().CompareTo(y.GetAppearBeat().GetBeatTime()));
        Factory.initiation();
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
            if (pattern[0].pivotType == "PlayerA")
            {
                for(int i = 0; i < Protocol.CONSTANTS.MAX_IN_GAME_PLAYER; ++i)
                {
                    Debug.Log("Player" + (i + 1));
                    HexCoordinates TargetPos = GameManager.data.GetTargetPos("Player"+(i+1));

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
                        case 7: // robot 보스 점프
                            EffectManager.instance.JumpAttack(charging_time, TargetPos, true);
                            break;
                        case 8: // robot 보스 총발사
                        TargetPos.W -= 1;

                            EffectManager.instance.GunAttack(charging_time, TargetPos);
                            break;
                        case 10:
                            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "InGameScene03")
                                EffectManager.instance.ElectricBallEffect(BossPos.getRealPosition(), ref GameManager.data.GetPlayerREF("Player" + (i + 1)), charging_time);

                            else
                                EffectManager.instance.BossTargetingEffect(BossPos.getRealPosition(), ref GameManager.data.GetPlayerREF(pattern[0].pivotType), charging_time);
                            break;


                        case 100: //장판 이펙트
                            EffectManager.instance.TileWaveEffect(TargetPos.X, TargetPos.Z, 5);
                            break;
                        case 101:
                            EffectManager.instance.TileRailWaveEffect(TargetPos.X, TargetPos.Z, 5, pattern[0].direction);
                            break;

                        case 200:   //아이템
                                    //print($"{TargetPos.X}, {TargetPos.Y}, {TargetPos.Z}");
                            switch (pattern[0].direction)
                            {
                                case HexDirection.Up:
                                    {
                                        var m = Instantiate(itemModels[0]);
                                        m.name = "coloring";

                                        GameManager.data.grid.cellMaps.Get(TargetPos.X, TargetPos.Y, TargetPos.Z).SetItemToThisCell(cellState.Item1, ref m);
                                        m.transform.localPosition = new Vector3(0, 2, 0);
                                    }
                                    break;
                                case HexDirection.Down:
                                    {
                                        var m = Instantiate(itemModels[1]);
                                        m.name = "coloring";

                                        GameManager.data.grid.cellMaps.Get(TargetPos.X, TargetPos.Y, TargetPos.Z).SetItemToThisCell(cellState.Item2, ref m);
                                        m.transform.localPosition = new Vector3(0, 2, 0);
                                    }
                                    break;
                                case HexDirection.LeftDown:
                                    {
                                        var m = Instantiate(itemModels[2]);
                                        m.name = "coloring";

                                        GameManager.data.grid.cellMaps.Get(TargetPos.X, TargetPos.Y, TargetPos.Z).SetItemToThisCell(cellState.Item3, ref m);
                                        m.transform.localPosition = new Vector3(0, 2, 0);
                                    }
                                    break;
                                case HexDirection.RightDown:
                                    {
                                        var m = Instantiate(itemModels[3]);
                                        m.name = "coloring";

                                        GameManager.data.grid.cellMaps.Get(TargetPos.X, TargetPos.Y, TargetPos.Z).SetItemToThisCell(cellState.Item4, ref m);
                                        m.transform.localPosition = new Vector3(0, 2, 0);
                                    }
                                    break;
                                case HexDirection.LeftUp:
                                    {
                                        var m = Instantiate(itemModels[4]);
                                        m.name = "coloring";

                                        GameManager.data.grid.cellMaps.Get(TargetPos.X, TargetPos.Y, TargetPos.Z).SetItemToThisCell(cellState.Item5, ref m);
                                        m.transform.localPosition = new Vector3(0, 2, 0);
                                    }
                                    break;
                                case HexDirection.RightUp:
                                    {
                                        var m = Instantiate(itemModels[5]);
                                        m.name = "coloring";

                                        GameManager.data.grid.cellMaps.Get(TargetPos.X, TargetPos.Y, TargetPos.Z).SetItemToThisCell(cellState.Item6, ref m);
                                        m.transform.localPosition = new Vector3(0, 2, 0);
                                    }
                                    break;
                            }


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

                }
                    settedPattern.Add(pattern[0]);
                    pattern.RemoveAt(0);
                    patNums++;
            }
            else
            {
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
                    case 7: // robot 보스 점프
                        EffectManager.instance.JumpAttack(charging_time, TargetPos, true);
                        break;
                    case 8: // robot 보스 총발사
                        TargetPos.W -= 1;
                        EffectManager.instance.GunAttack(charging_time, TargetPos);
                        break;
                    case 10:
                        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "InGameScene03")
                            EffectManager.instance.ElectricBallEffect(BossPos.getRealPosition(), ref GameManager.data.GetPlayerREF(pattern[0].pivotType), charging_time);

                        else
                            EffectManager.instance.BossTargetingEffect(BossPos.getRealPosition(), ref GameManager.data.GetPlayerREF(pattern[0].pivotType), charging_time);
                        break;


                    case 100: //장판 이펙트
                        EffectManager.instance.TileWaveEffect(TargetPos.X, TargetPos.Z, 5);
                        break;
                    case 101:
                        EffectManager.instance.TileRailWaveEffect(TargetPos.X, TargetPos.Z, 5, pattern[0].direction);
                        break;

                    case 200:   //아이템
                                //print($"{TargetPos.X}, {TargetPos.Y}, {TargetPos.Z}");
                        switch (pattern[0].direction)
                        {
                            case HexDirection.Up:
                                {
                                    var m = Instantiate(itemModels[0]);
                                    m.name = "coloring";

                                    GameManager.data.grid.cellMaps.Get(TargetPos.X, TargetPos.Y, TargetPos.Z).SetItemToThisCell(cellState.Item1, ref m);
                                    m.transform.localPosition = new Vector3(0, 2, 0);
                                }
                                break;
                            case HexDirection.Down:
                                {
                                    var m = Instantiate(itemModels[1]);
                                    m.name = "coloring";

                                    GameManager.data.grid.cellMaps.Get(TargetPos.X, TargetPos.Y, TargetPos.Z).SetItemToThisCell(cellState.Item2, ref m);
                                    m.transform.localPosition = new Vector3(0, 2, 0);
                                }
                                break;
                            case HexDirection.LeftDown:
                                {
                                    var m = Instantiate(itemModels[2]);
                                    m.name = "coloring";

                                    GameManager.data.grid.cellMaps.Get(TargetPos.X, TargetPos.Y, TargetPos.Z).SetItemToThisCell(cellState.Item3, ref m);
                                    m.transform.localPosition = new Vector3(0, 2, 0);
                                }
                                break;
                            case HexDirection.RightDown:
                                {
                                    var m = Instantiate(itemModels[3]);
                                    m.name = "coloring";

                                    GameManager.data.grid.cellMaps.Get(TargetPos.X, TargetPos.Y, TargetPos.Z).SetItemToThisCell(cellState.Item4, ref m);
                                    m.transform.localPosition = new Vector3(0, 2, 0);
                                }
                                break;
                            case HexDirection.LeftUp:
                                {
                                    var m = Instantiate(itemModels[4]);
                                    m.name = "coloring";

                                    GameManager.data.grid.cellMaps.Get(TargetPos.X, TargetPos.Y, TargetPos.Z).SetItemToThisCell(cellState.Item5, ref m);
                                    m.transform.localPosition = new Vector3(0, 2, 0);
                                }
                                break;
                            case HexDirection.RightUp:
                                {
                                    var m = Instantiate(itemModels[5]);
                                    m.name = "coloring";

                                    GameManager.data.grid.cellMaps.Get(TargetPos.X, TargetPos.Y, TargetPos.Z).SetItemToThisCell(cellState.Item6, ref m);
                                    m.transform.localPosition = new Vector3(0, 2, 0);
                                }
                                break;
                        }


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
