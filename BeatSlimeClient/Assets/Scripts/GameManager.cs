using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager data;

    public int mapCellid = 0;

    public PatternManager PM;

    public GameObject player;
    public GameObject enemy;
    public GameObject tilemap;
    public HexGrid grid;
    public MapLoader loader;

    public SoundManager soundManager;
    public SoundEffectManager soundEffectManager;
    public SoundProgressBar soundProgressBar;

    public BeatManager beatManager;

    public bool isGameStart;
    bool debugStart;

    public string SongName;

    public float bpm;
    public int barCounts = 4;
    public int totalSongTime;
    public int nowSongTime;
    public int timeByBar;
    public int timeByBeat;
    public int timeBy24Beat;
    public int timeBy16Beat;
    //public int nowBars;
    //public int nowBeats;
    //public int now24Beats;
    //public int now32Beats;
    public Beat nowBeat;
    public int beatCounter = 0;

    public int offsetTime;

    public int JudgementTiming;

    public int alreadyMoved;

    public DoomChi MidNote;
    public DoomChi MidANote;

    public HPImagesForGameManager HPGM;

    public GameObject JudgeEffect;
    public UIJudgeEffect ComboEffect;
    public SlimeJudgeEffect ParticleEffect;
    int nowCombo;
    int HPGMStaticInt = 1;

    public GameOverImage gameOverImage;
    public ResultsData resultsData;

    //
    GameObject[] Objects = new GameObject[4];
    int[] ids;
    int myPlayerID = -1;
    public ArrayList Mapdata = new ArrayList();

    //server time
    int GameStartTime;
    int GameElapsedTime;
    int[] ping_data = {-1,-1,-1,-1,-1, -1,-1,-1,-1,-1};
    int ping_index;
    void Awake()
    {
        print("Start");
        nowCombo = 0;
        //HPGM = gameObject.GetComponent<HPImagesForGameManager>();
        data = this;
        isGameStart = false;
        debugStart = false;
        alreadyMoved = 0;

        if(grid.TMP)
        {
            loader.Match(grid);
            loader.LoadMap();
        }

        resultsData = new ResultsData();
        nowBeat = new Beat();

        bpm = SoundManager.instance.GetBGMBpm(SongName);
        totalSongTime = (int)(SoundManager.instance.GetMusicLength(SongName) * 1000f);
        nowSongTime = 0;
        timeByBeat = (int)(1000f * 60f / bpm);
        timeByBar = timeByBeat * barCounts;
        timeBy24Beat = timeByBeat / 6;
        timeBy16Beat = timeByBeat / 4;

        player.GetComponent<PlayerManager>().SetHPImages(HPGM.HPs[0], HPGM.prevHPs[0]);
    }
    private void OnApplicationQuit()
    {
        FieldGameManager.Net.CloseSocket();
    }
    void Start()
    {
        if (!FieldGameManager.Net.isServerOnline()) FieldGameManager.Net.CreateAndConnect();
        FieldGameManager.Net.SendChangeSceneDonePacket(1);
        loader.LoadMap();

        //DEBUG
        enemy.GetComponent<HexCellPosition>().setInitPosition(0, 0);
        PM = PatternManager.data;
        ids = new int[4];
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown("9"))
        {
            JudgeEffect.GetComponent<IndicatorJudgeEffect>().JudgeApply(JudgeCases.PERFECT);
            ParticleEffect.ParticleApply(JudgeCases.PERFECT);
            ComboEffect.CountApply(ref nowCombo);
        }
        //if (Input.GetKeyDown("1"))
        //{
        //if (Mapdata.Count != 0)
        //FieldGameManager.Net.SendWriteMapPacket(Mapdata);
        //}
        //else if (Input.GetKeyDown(KeyCode.BackQuote))
        //{
        //    FieldGameManager.Net.SendreadPacket();
        //}
        //if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    FieldGameManager.Net.SendGameStartReadyPacket();
        //}


        if (isGameStart && !FieldGameManager.Net.isServerOnline())
        {
            int prevBeats = nowBeat.addBeat;

            nowSongTime = SoundManager.instance.GetMusicLapsedTime() - offsetTime;
            if (nowSongTime > 0)
            {
                nowBeat.SetBeatTime(nowSongTime);

                //Debug.Log(nowBeat.ToString() + " : now");

                beatCounter -= (int)(Time.deltaTime * 1000f);

                PM.PeekPattern(nowBeat);
                PM.ServePattern(nowBeat);

                if (prevBeats != nowBeat.addBeat)
                {
                    player.GetComponent<PlayerManager>().Beat();
                    enemy.GetComponent<EnemyManager>().Beat();
                    grid.Beat();
                    soundEffectManager.BeatEffect();
                    beatCounter = timeByBeat;
                }
            }

            if (alreadyMoved > 0)
                alreadyMoved -= (int)(Time.deltaTime * 1000f);
        }
        else if (isGameStart && FieldGameManager.Net.isServerOnline())
        {
            int prevBeats = nowBeat.addBeat;

            nowSongTime = SoundManager.instance.GetMusicLapsedTime() - offsetTime;
            if (nowSongTime > 0)
            {
                nowBeat.SetBeatTime(nowSongTime);

                //Debug.Log(nowBeat.ToString() + " : now");

                beatCounter -= (int)(Time.deltaTime * 1000f);

                PM.PeekPattern(nowBeat);
                PM.ServePattern(nowBeat);

                if (prevBeats != nowBeat.addBeat)
                {
                    Objects[myPlayerID].GetComponentInChildren<PlayerManager>().Beat();
                    enemy.GetComponent<EnemyManager>().Beat();
                    grid.Beat();
                    soundEffectManager.BeatEffect();
                    beatCounter = timeByBeat;
                }
            }

            if (alreadyMoved > 0)
                alreadyMoved -= (int)(Time.deltaTime * 1000f);
        }
        if (FieldGameManager.Net.isServerOnline())
        {
            //
            while (Network.MessQueue.Count > 0)
            {
                byte[] data = Network.MessQueue.Dequeue();

                byte type = data[1];

                switch (type)
                {
                    case Protocol.CONSTANTS.SC_PACKET_CHANGE_SCENE:
                        {
                            Protocol.sc_packet_change_scene p = Protocol.sc_packet_change_scene.SetByteToVar(data);
                            UnityEngine.SceneManagement.SceneManager.LoadScene(p.scene_num);
                            Debug.Log("Game_Over -> Change Scene");
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_GAME_INIT:
                        {
                            Protocol.sc_packet_game_init p = Protocol.sc_packet_game_init.SetByteToVar(data);
                            Debug.Log("game init");
                            ids[0] = p.id1;
                            ids[1] = p.id2;
                            ids[2] = p.id3;
                            ids[3] = p.boss_id;
                            int pid = ServerID_To_ClientID(p.player_id);

                            Objects[pid] = player;
                            myPlayerID = pid;
                            Objects[3] = enemy;
                            Objects[3].SetActive(true);

                            PatternManager.data.Load(myPlayerID);

                            FieldGameManager.Net.SendGameStartReadyPacket();
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_GAME_START:
                        {
                            Protocol.sc_packet_game_start p = Protocol.sc_packet_game_start.SetByteToVar(data);

                            GameStartTime = p.game_start_time;
                            PlaySound();
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_MOVE:
                        {
                            Protocol.sc_packet_move p = Protocol.sc_packet_move.SetByteToVar(data);

                            Debug.Log("MOVE PACKET x : " + p.x);

                            int pid = ServerID_To_ClientID(p.id);
                            
                            if (!debugStart)
                            {
                                
                                //Debug.Log(p.id+" : pid");
                                Objects[pid].GetComponentInChildren<HexCellPosition>().setDirection((byte)p.dir);
                                Objects[pid].GetComponentInChildren<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                if (pid == myPlayerID)// 
                                {
                                    Objects[pid].GetComponentInChildren<PlayerManager>().JumpTrig();

                                }
                                else if (pid < 3)   //
                                    Objects[pid].GetComponentInChildren<PlayerManager>().JumpTrig();
                            }
                            else    //debug time
                            {
                                player.GetComponent<HexCellPosition>().setDirection((byte)p.dir);
                                player.GetComponent<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                player.GetComponent<PlayerManager>().JumpTrig();
                            }


                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_ATTACK:
                        {
                            Protocol.sc_packet_attack p = Protocol.sc_packet_attack.SetByteToVar(data);

                            int target_id = ServerID_To_ClientID(p.target_id);
                            //int randomTickForTest = 4;//Random.Range(1, 6);
                            //enemy.GetComponent<EnemyManager>().BeatPatternServe(nowBeat, new Beat(0, randomTickForTest), Objects[target_id]);
                            //Objects[target_id].GetComponent<PlayerManager>().SetBallBeat(nowBeat, new Beat(0, randomTickForTest));
                            //Debug.Log("ServerID_To_ClientID : " + p.target_id+ " to " + target_id);
                            if(target_id != 3) // is not boss
                            {
                                HPManager hm = Objects[target_id].GetComponentInChildren<PlayerManager>().HP;
                                
                                Objects[target_id].GetComponentInChildren<PlayerManager>().StunTrig();
                                //Debug.Log("ID : " + p.target_id + "damage : " + (hm.CurrentHP - p.hp));

                                //Debug.Log("ATTACK : " + target_id + ", HP : " + hm.CurrentHP +" to " + p.hp);
                                if (target_id == myPlayerID)
                                {
                                    resultsData.damaged += (hm.CurrentHP - p.hp);
                                    
                                    CineCameraShake.instance.ShakeCamera(hm.CurrentHP - p.hp);
                                }

                                hm.Damage(hm.CurrentHP - p.hp);

                                
                            }
                            else
                            {
                                //Objects[ServerID_To_ClientID(p.id)].GetComponentInChildren<PlayerManager>().AttackTrig();
                                HPManager hm = Objects[target_id].GetComponentInChildren<EnemyManager>().HP;
                                resultsData.attack += (hm.CurrentHP - p.hp);
                                
                                hm.Damage(hm.CurrentHP - p.hp);

                                
                                //Objects[target_id].GetComponentInChildren<EnemyManager>().StunTrig();
                            }

                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PUT_OBJECT:
                        {
                            Protocol.sc_packet_put_object p = Protocol.sc_packet_put_object.SetByteToVar(data);
                            int pid = ServerID_To_ClientID(p.id);
                            if (pid == myPlayerID)
                            {
                                Objects[pid].GetComponentInChildren<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                break;
                            }
                            //PutObject(p.type, p.id, p.x, p.y);
                            switch (p.obj_type)
                            {
                                case (byte)Protocol.OBJECT_TYPE.PLAPER:
                                    {
                                        if (HPGMStaticInt <= 2)
                                        {
                                            // Debug.Log(p.id + ", " + p.x + ", " + p.y + ", " + p.z + ", " + "ï¿½Ã·ï¿½ï¿½Ì¾ï¿½ ï¿½ï¿½ï¿½ï¿½");
                                            Objects[pid] = ObjectPool.instance.PlayerObjectQueue.Dequeue();
                                            Objects[pid].SetActive(true);
                                            Objects[pid].GetComponentInChildren<PlayerManager>().SetHPImages(HPGM.HPs[HPGMStaticInt], GameManager.data.HPGM.prevHPs[HPGMStaticInt]);
                                            HPGMStaticInt++;

                                            Objects[pid].GetComponentInChildren<Animator>().SetFloat("Speed", bpm / 45.0f);

                                            Objects[pid].GetComponentInChildren<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                        }
                                        break;
                                    }
                                case (byte)Protocol.OBJECT_TYPE.ENEMY:
                                    {
                                        // Debug.Log(p.id + ", " + p.x + ", " + p.y + ", " + p.z + ", " + "ï¿½Ã·ï¿½ï¿½Ì¾ï¿½ ï¿½ï¿½ï¿½ï¿½");
                                        Objects[pid] = enemy;
                                        Objects[pid].SetActive(true);
                                        Objects[pid].GetComponentInChildren<Animator>().SetFloat("Speed", bpm / 45.0f);

                                        Objects[pid].GetComponentInChildren<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                        break;
                                    }
                            }

                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_REMOVE_OBJECT:
                        {
                            Protocol.sc_packet_remove_object p = Protocol.sc_packet_remove_object.SetByteToVar(data);
                            int pid = ServerID_To_ClientID(p.id);

                            if (pid < 3)
                            {
                                ObjectPool.instance.PlayerObjectQueue.Enqueue(Objects[pid]);
                                Objects[pid].SetActive(false);
                            }
                            else
                            {
                                ObjectPool.instance.EnemyObjectQueue.Enqueue(Objects[pid]);
                                Objects[pid].SetActive(false);
                            }

                            //ReMoveObject(p.id);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_MAP_DATA:
                        {
                            int size = data[0];
                            Debug.Log(size);
                            int index = 0;

                            Mapdata.Clear();

                            while (index < size - 2)
                            {
                                Protocol.Map shell = Protocol.Map.SetByteToMap(data, index + 2);
                                index += 28;
                                Mapdata.Add(shell);
                            }
                            foreach (Protocol.Map s in Mapdata)
                            {
                                grid.P_MakeHexMap(s);
                                Debug.Log(s.x + ", " + s.y + ", " + s.z + ", " + s.color + ", " + s.type);
                            }

                            //ReMoveObject(p.id);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_EFFECT:
                        {
                            Protocol.sc_packet_effect p = Protocol.sc_packet_effect.SetByteToVar(data);
                            int pid = ServerID_To_ClientID(p.id);
                            int tid = -1;
                            if (p.target_id != -1)
                                tid = ServerID_To_ClientID(p.target_id);
                            
                            // ÇÃ·¹ÀÌ¾î ½ºÅ³ ÀÌÆåÆ®¸¸ ¿©±â¼­ Ãâ·Â ¹øÈ£´Â 55¹ø
                            switch (p.effect_type)
                            {
                                case 3:
                                    EffectManager.instance.BossTileEffect(p.x, p.y, p.z, p.charging_time,3);
                                    break;
                                case 4:
                                    EffectManager.instance.BossTileEffect(p.x, p.y, p.z, p.charging_time,4);
                                    break;
                                case 99:
                                    EffectManager.instance.OneTileEffect(p.x, p.y, p.z, p.charging_time);
                                    break;
                                case 5:
                                    EffectManager.instance.BossWaterGunEffect(Objects[pid].transform.localPosition, Objects[tid].transform.localPosition, p.charging_time);
                                break;
                                case 6:
                                    EffectManager.instance.BossQuakeEffect(p.x, p.y, p.z, p.charging_time, Objects[tid].GetComponent<HexCellPosition>().direction);
                                    break;
                                case 10:
                                    EffectManager.instance.BossTargetingEffect(Objects[pid].transform.localPosition,ref Objects[tid], p.charging_time);
                                    break;

                                case 55:// skill
                                    // p->x  == skill Level
                                    // p->y  == skill type
                                    switch (p.y)
                                    {
                                        case 1:
                                            EffectManager.instance.PlayerWaterGunEffect(Objects[pid].transform.localPosition, ref Objects[tid], p.charging_time);

                                            break;
                                        case 2:
                                            {
                                                HexCoordinates cell = Objects[pid].GetComponent<HexCellPosition>().coordinates;
                                                EffectManager.instance.PlayerQuakeEffect(cell.X,cell.Y,cell.Z, p.charging_time);
                                                //Debug.Log(cell.X + ", " + cell.Y + ", " + cell.Z + " skill attack");

                                            }
                                            break;
                                        case 3:
                                            {
                                                HexCoordinates cell = Objects[pid].GetComponent<HexCellPosition>().coordinates;
                                                EffectManager.instance.PlayerHealEffect(cell.X, cell.Y, cell.Z, p.charging_time);
                                                //Debug.Log(cell.X + ", " + cell.Y + ", " + cell.Z + " skill attack");

                                            }
                                            break;
                                    }
                                    Debug.Log(p.y + " skill type");
                                    break;
                            }
                            //StartCoroutine(EffectManager.instance.TileEffect0(0, 0, 0, 0,HexDirection.LeftDown));

                            //Debug.Log(start_x+ " " + start_y + " " + start_z + " " + start_w);
                            //EffectManager.instance.BossTileEffect1(0, 0, 0, 0);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_GAME_END:
                        {
                            Protocol.sc_packet_game_end p = Protocol.sc_packet_game_end.SetByteToVar(data);
                            isGameStart = false;

                            if (p.end_type == 0)
                            {
                                gameOverImage.SetGameEnd(GameEndTraits.Lose);
                                gameOverImage.SetResultData(resultsData.perfect, resultsData.great, resultsData.miss, resultsData.attack,resultsData.damaged,0);
                                Debug.Log("Game_Over");
                            }
                            else if (p.end_type == 1)
                            {
                                gameOverImage.SetGameEnd(GameEndTraits.Win);
                                gameOverImage.SetResultData(resultsData.perfect, resultsData.great, resultsData.miss, resultsData.attack,resultsData.damaged,
                                                            resultsData.perfect*1000 + resultsData.great*500 - resultsData.miss*50 + resultsData.attack*100 - resultsData.damaged*20);
                                Debug.Log("Game_Clear");
                            }
                            else
                            {
                                Debug.Log("Game_Error");
                            }
                            
                            
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PARRYING:
                        {
                            Protocol.sc_packet_parrying p = Protocol.sc_packet_parrying.SetByteToVar(data);
                            
                            int pid = ServerID_To_ClientID(p.id);
                            Objects[pid].GetComponentInChildren<PlayerManager>().ParryTrig();

                            MidANote.notePerfect();
                            JudgeEffect.GetComponent<IndicatorJudgeEffect>().JudgeApply(JudgeCases.PERFECT);
                            ParticleEffect.ParticleApply(JudgeCases.PERFECT);
                            nowCombo++;
                            Debug.Log("Parrying Success");
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_ELAPSED_TIME:
                        {
                            Protocol.sc_packet_elapsed_time p = Protocol.sc_packet_elapsed_time.SetByteToVar(data);

                            GameElapsedTime = p.elapsed_time;
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PING_TEST:
                        {
                            Protocol.sc_packet_ping_test p = Protocol.sc_packet_ping_test.SetByteToVar(data);
                            
                            int ping = System.DateTime.Now.Millisecond - p.ping_time;
                            ping_data[ping_index++] = ping;
                            ping_index = ping_index % ping_data.Length;
                            Debug.Log(ping);
                        }
                        break;
                    default:
                        Debug.Log("wrong + type");

                        break;
                }
            }
        }
    }
    //IEnumerator AutoPatternSetter()
    //{
    //    while (isGameStart)
    //    {
    //        PM.PeekPattern(nowBeat);
    //        PM.ServePattern(nowBeat);
    //        yield return null;
    //    }
    //}
    int ServerID_To_ClientID(int id)
    {
        for (int i = 0; i < 4; ++i)
        {
            if (id == ids[i]) return i;
        }
        Debug.Log("wrong id");
        return -1;
    }
    public void PlaySound()
    {

        if (!isGameStart)
        {
            soundManager.PlayBGM(SongName);
            soundEffectManager.BeatEffect();
            beatCounter = timeByBeat;
            isGameStart = true;
            debugStart = false;
        }
        //StartCoroutine(AutoPatternSetter());

    }

    public int getIsGoodTiming()
    {
        if (alreadyMoved <= 0)
        {
            if (beatCounter <= JudgementTiming)
            {
                MidNote.notePerfect();
                //DEBUG ÆÛÆåÆ® ÆÄÆ¼Å¬ Ãß°¡

                if (beatCounter <= JudgementTiming/2f)
                {
                    resultsData.perfect++;
                    JudgeEffect.GetComponent<IndicatorJudgeEffect>().JudgeApply(JudgeCases.PERFECT);
                    ParticleEffect.ParticleApply(JudgeCases.PERFECT);
                    ComboEffect.CountApply(ref nowCombo);
                }
                else
                {
                    resultsData.great++;
                    JudgeEffect.GetComponent<IndicatorJudgeEffect>().JudgeApply(JudgeCases.GOOD);
                    ParticleEffect.ParticleApply(JudgeCases.GOOD);
                    ComboEffect.CountApply(ref nowCombo);
                }

                //DEBUG : ï¿½Ð¸ï¿½ ï¿½ï¿½Æ®ï¿½ï¿½ï¿½ï¿½ ï¿½Æ·ï¿½ ï¿½Úµå¸¦
                //MidANote.notePerfect();
                return 1;  //ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
            }
            else if (timeByBeat - beatCounter <= JudgementTiming)
            {
                MidNote.notePerfect();

                if ((timeByBeat - beatCounter) <= JudgementTiming/2f)
                {
                    resultsData.perfect++;
                    JudgeEffect.GetComponent<IndicatorJudgeEffect>().JudgeApply(JudgeCases.PERFECT);
                    ParticleEffect.ParticleApply(JudgeCases.PERFECT);
                    ComboEffect.CountApply(ref nowCombo);
                }
                else
                {
                    resultsData.great++;
                    JudgeEffect.GetComponent<IndicatorJudgeEffect>().JudgeApply(JudgeCases.GOOD);
                    ParticleEffect.ParticleApply(JudgeCases.GOOD);
                    ComboEffect.CountApply(ref nowCombo);
                }

                return 1;   //ï¿½Ê°ï¿½ ï¿½ï¿½ï¿½ï¿½
            }
            else
            {
                //MidNote.noteEnd();
                Debug.Log("Error Beside : -" + beatCounter + ", +" + (timeByBeat - beatCounter));
                resultsData.miss++;
                JudgeEffect.GetComponent<IndicatorJudgeEffect>().JudgeApply(JudgeCases.BAD);
                //ParticleEffect.ParticleApply(JudgeCases.BAD);
                ComboEffect.CountApply(ref nowCombo,true);
                return 0;   //ï¿½ß¸ï¿½ ï¿½ï¿½ï¿½ï¿½
            }
        }
        else
        {
            ComboEffect.CountApply(ref nowCombo,true);
            //MidNote.noteEnd();
            //Debug.Log("Cutting");
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

        alreadyMoved = JudgementTiming * 2;
    }

}
