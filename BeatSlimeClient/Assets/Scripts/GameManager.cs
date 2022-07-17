using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GameManager : MonoBehaviour
{
    public static GameManager data;

    [System.NonSerialized]
    public int mapCellid = 0;

    public PatternManager PM;

    public GameObject player;
    public GameObject enemy;
    public GameObject enemyParriedTarget;
    Animator enemyAnim;
    public GameObject tilemap;
    public HexGrid grid;
    public MapLoader loader;

    public SoundManager soundManager;


    public bool isGameStart;
    bool debugStart;

    public string SongName;

    [System.NonSerialized]
    public float bpm;
    public int barCounts = 4;

    [System.NonSerialized]
    public int totalSongTime;

    [System.NonSerialized]
    public int nowSongTime;

    [System.NonSerialized]
    public int timeByBar;

    [System.NonSerialized]
    public int timeByBeat;

    [System.NonSerialized]
    public int timeBy24Beat;

    [System.NonSerialized]
    public int timeBy16Beat;
    //public int nowBars;
    //public int nowBeats;
    //public int now24Beats;
    //public int now32Beats;
    public Beat nowBeat;

    [System.NonSerialized]
    public int beatCounter = 0;

    private int offsetTime;
    public int offsetMusicFrontTime;
    public int offsetPatternSinker;
    public int offsetMusicSinker;

    public int JudgementTiming;

    [System.NonSerialized]
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
    [System.NonSerialized]
    public GameObject[] Objects = new GameObject[4];
    public int[] ids;
    public int myPlayerID = -1;
    public ArrayList Mapdata = new ArrayList();

    //server time
    int GameStartTime;

    int avgPing;
    float prePingTestTime;

    [System.NonSerialized]
    public float MusicStartOffset;


    public MusicName MN;
    public PlayableDirector PD;

    public bool isTutorial;

    void Awake()
    {

        print("Start");
        nowCombo = 0;
        //HPGM = gameObject.GetComponent<HPImagesForGameManager>();
        data = this;
        isGameStart = false;
        debugStart = false;
        alreadyMoved = 0;

        if (grid.TMP)
        {
            loader.Match(ref grid);
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

        enemyAnim = enemy.GetComponentInChildren<Animator>();
       
        //¿©±â¼­ ÇÒ ¼ö¹Û¿¡ ¾øÀ½
        player.GetComponentInChildren<PlayerManager>().SetHPImages(HPGM.PlayerHPs[0].HPImage, HPGM.PlayerHPs[0].prevHPImage);
        HPGM.PlayerHPs[0].Set(FieldPlayerManager.self_skillnum, FieldPlayerManager.myName);
    }
    private void OnApplicationQuit()
    {
        Network.CloseSocket();
    }
    public Animator GetEnemyAnim()
    {
        return enemyAnim;
    }
    void Start()
    {
        if (!Network.isServerOnline()) Network.CreateAndConnect();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "InGameScene01")
            Network.SendChangeSceneDonePacket(1);
        else
            Network.SendChangeSceneDonePacket(2);
        offsetTime = 0;
        prePingTestTime = 5.0f;
        //DEBUG
        enemy.GetComponent<HexCellPosition>().setInitPosition(0, 0);
        PM = PatternManager.data;
        ids = new int[4];




    }
    public HexDirection GetBossDir()
    {
        return Objects[3].GetComponent<HexCellPosition>().direction;
    }
    int FindMinHpID()
    {
        int min = 0xfffff;
        int ret = -1;
        for (int i = 0; i < Protocol.CONSTANTS.MAX_IN_GAME_PLAYER; ++i)
        {
            if (false == Objects[i].activeSelf) continue;
            int hp = Objects[i].GetComponentInChildren<PlayerManager>().HP.CurrentHP;
            if (min > hp)
            {
                min = hp;
                ret = i;
            }
        }
        return ret;
    }
    int FindMaxHpID()
    {
        int max = 0;
        int ret = -1;
        for (int i = 0; i < Protocol.CONSTANTS.MAX_IN_GAME_PLAYER; ++i)
        {
            if (false == Objects[i].activeSelf) continue;

            int hp = Objects[i].GetComponentInChildren<PlayerManager>().HP.CurrentHP;
            if (max < hp)
            {
                max = hp;
                ret = i;
            }
        }
        return ret;
    }

    int FindMinDistanceHpID()
    {
        float min = 99999;
        int ret = -1;
        for (int i = 0; i < Protocol.CONSTANTS.MAX_IN_GAME_PLAYER; ++i)
        {
            if (false == Objects[i].activeSelf) continue;

            float distance = Vector3.Distance(Objects[3].transform.position, Objects[i].transform.position);
            if (min > distance)
            {
                min = distance;
                ret = i;
            }
        }
        return ret;
    }
    int FindMaxDistanceHpID()
    {
        float max = 0;
        int ret = -1;
        for (int i = 0; i < Protocol.CONSTANTS.MAX_IN_GAME_PLAYER; ++i)
        {
            if (false == Objects[i].activeSelf) continue;

            float distance = Vector3.Distance(Objects[3].transform.position, Objects[i].transform.position);

            if (max < distance)
            {
                max = distance;
                ret = i;
            }
        }
        return ret;
    }
    int FindOnlineID()
    {
        for (int i = 0; i < Protocol.CONSTANTS.MAX_IN_GAME_PLAYER; ++i)
        {
            if (false == Objects[i].activeSelf) continue;
            return i;
        }
        return -1;
    }
    public HexCoordinates GetBossPos()
    {
        return Objects[3].GetComponent<HexCellPosition>().coordinates;
    }

    public HexCoordinates GetTargetPos(string pivotType)
    {
        HexCoordinates target_pos = new HexCoordinates();
        int OnlinePlayerID = FindOnlineID();


        // All die
        if (OnlinePlayerID == -1)
        {
            return new HexCoordinates(0, 0, 0);
        }
        if (pivotType == "PlayerM")
        {
            target_pos = Objects[FindMaxHpID()].GetComponent<HexCellPosition>().coordinates;
        }
        else if (pivotType == "Playerm")
        {
            target_pos = Objects[FindMinHpID()].GetComponent<HexCellPosition>().coordinates;
        }
        else if (pivotType == "PlayerF")
        {
            // Debug.Log("PlayerF : " + FindMaxDistanceHpID());
            target_pos = Objects[FindMaxDistanceHpID()].GetComponent<HexCellPosition>().coordinates;
        }
        else if (pivotType == "PlayerN")
        {

            //Debug.Log("PlayerN : " + FindMinDistanceHpID());

            target_pos = Objects[FindMinDistanceHpID()].GetComponent<HexCellPosition>().coordinates;
        }
        else if (pivotType == "World")
        {
            target_pos = new HexCoordinates(0, 0, 1);
        }
        else if (pivotType == "Boss")
        {
            target_pos = Objects[3].GetComponent<HexCellPosition>().coordinates;
        }// ¼öÁ¤
        else if (pivotType == "Player1")// || pivotType == "Player2"|| pivotType == "Player3")
        {
            if (Objects[0].activeSelf)
            {
                target_pos = Objects[0].GetComponent<HexCellPosition>().coordinates;
            }
            else
            {
                target_pos = Objects[OnlinePlayerID].GetComponent<HexCellPosition>().coordinates;
            }
        }
        else if (pivotType == "Player2")
        {
            if (Objects[1].activeSelf)
            {
                target_pos = Objects[1].GetComponent<HexCellPosition>().coordinates;
            }
            else
            {
                target_pos = Objects[OnlinePlayerID].GetComponent<HexCellPosition>().coordinates;
            }
        }
        else if (pivotType == "Player3")
        {
            if (Objects[2].activeSelf)
            {
                target_pos = Objects[2].GetComponent<HexCellPosition>().coordinates;
            }
            else
            {
                target_pos = Objects[OnlinePlayerID].GetComponent<HexCellPosition>().coordinates;
            }
        }
        else if (pivotType == "END")
            Debug.Log("Pattern End");
        else
            Debug.Log("Load : wrong pivotType " + pivotType);

        return target_pos;
    }
    public ref GameObject GetPlayerREF(string pivotType)
    {
        ref GameObject target = ref Objects[0];
        int OnlinePlayerID = FindOnlineID();
        if (OnlinePlayerID == -1)
        {
            Debug.Log("All die");
            return ref target;
        }
        if (pivotType == "PlayerM")
        {
            target = ref Objects[FindMaxHpID()];
        }
        else if (pivotType == "Playerm")
        {
            target = ref Objects[FindMinHpID()];
        }

        else if (pivotType == "Boss")
        {
            target = ref Objects[3];
        }
        else if (pivotType == "Player1")// || pivotType == "Player2"|| pivotType == "Player3")
        {
            if (Objects[0].activeSelf)
            {
                target = ref Objects[0];
            }
            else
            {
                target = ref Objects[OnlinePlayerID];
            }
        }
        else if (pivotType == "Player2")
        {
            if (Objects[1].activeSelf)
            {
                target = ref Objects[1];
            }
            else
            {
                target = ref Objects[OnlinePlayerID];
            }
        }
        else if (pivotType == "Player3")
        {
            if (Objects[2].activeSelf)
            {
                target = ref Objects[2];
            }
            else
            {
                target = ref Objects[OnlinePlayerID];
            }
        }
        return ref target;
    }

    bool ___cord = false;
    int zxcv = 1;
    void Update()
    {
        // if (Input.GetKeyDown("9"))
        // {
        //     enemyAnim.SetTrigger("Attack");
        // }
        // if (Input.GetKeyDown("8"))
        // {
        //     enemyAnim.SetTrigger("Move");
        //     //player.GetComponent<PlayerManager>().JumpTrigger.SetTrigger("Dead");
        // }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            
            HexCoordinates h = new HexCoordinates(5* zxcv, -3* zxcv,1);
            zxcv *= -1;
            
            EffectManager.instance.JumpAttack(10000,h,true);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            PM.Factory.scrollSpeed += 0.05f;
            PM.Factory.scrollUpdate();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            PM.Factory.scrollSpeed -= 0.05f;
            PM.Factory.scrollUpdate();
        }
        //if (Input.GetKeyDown("1"))
        //{
        //if (Mapdata.Count != 0)
        //Network.SendWriteMapPacket(Mapdata);
        //}
        //else if (Input.GetKeyDown(KeyCode.BackQuote))
        //{
        //    Network.SendreadPacket();
        //}
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Network.SendTeleportPacket(2);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            VFXManager.data.HitSounder(1);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Network.SendTeleportPacket(4);
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            player.GetComponentInChildren<PlayerManager>().changeColor(Color.red);
        }

        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                Network.SendGetItemPacket(0);
                
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
               
                Network.SendGetItemPacket(1);
                
            }
            if (Input.GetKeyDown(KeyCode.F10))
            {
               
                Network.SendGetItemPacket(2);
               
            }
            if (Input.GetKeyDown(KeyCode.F11))
            {
               
                Network.SendGetItemPacket(3);
              
            }
            if (Input.GetKeyDown(KeyCode.F12))
            {
                
                Network.SendGetItemPacket(4);
            }
            
        }


        if (isGameStart && Network.isServerOnline())
        {
            int prevBeats = nowBeat.addBeat;

            if (!___cord)
            {
                ___cord = true;
                Debug.Log("LAPSED TIME : " + SoundManager.instance.GetMusicLapsedTime());
                MusicStartOffset = SoundManager.instance.GetMusicLapsedTime() + offsetPatternSinker;
            }

            nowSongTime = SoundManager.instance.GetMusicLapsedTime() + offsetTime - (int)MusicStartOffset - offsetMusicFrontTime - offsetMusicSinker;
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
                    beatCounter = timeByBeat;
                }
            }

            if (alreadyMoved > 0)
                alreadyMoved -= (int)(Time.deltaTime * 1000f);
            prePingTestTime -= Time.deltaTime;
            if (prePingTestTime < 0)
            {
                prePingTestTime = 5.0f;
                Network.SendPingTestPacket();
            }
        }
        if (Network.isServerOnline())
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
                            // p->cool_times[0],[1],[2] ÄðÅ¸ÀÓ
                            ids[0] = p.id1;
                            ids[1] = p.id2;
                            ids[2] = p.id3;
                            ids[3] = p.boss_id;
                            int pid = ServerID_To_ClientID(p.player_id);
                            

                            print("init pid : " +pid);
                            Objects[pid] = player;
                            myPlayerID = pid;
                            Objects[3] = enemy;
                            Objects[3].SetActive(true);


                            if (!isTutorial)
                            {
                                int currenty = 1;
                                for (int i=0;i<3;i++)
                                {
                                    if (pid == i)
                                    {
                                        HPGM.PlayerHPs[0].cooltime = (float)p.cool_times[i];
                                    }
                                    else
                                    {
                                        HPGM.PlayerHPs[currenty].cooltime = (float)p.cool_times[i];
                                        currenty++;
                                    }
                                }
                            }
                            else
                            {
                                HPGM.PlayerHPs[0].cooltime = (float)p.cool_times[pid];
                            }


                            PatternManager.data.Load(myPlayerID);

                            Network.SendGameStartReadyPacket();
                            Network.SendPingTestPacket();
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_GAME_START:
                        {
                            Protocol.sc_packet_game_start p = Protocol.sc_packet_game_start.SetByteToVar(data);

                            GameStartTime = p.game_start_time;
                            int Delay = System.DateTime.Now.Millisecond;

                            PlaySound();
                            if (PD)
                                PD.Play();
                            MN.ChangeMusicName(SongName + " - zeroste.");
                            player.GetComponentInChildren<Animator>().SetFloat("Speed", PlayerPrefs.GetFloat("pAnimSpeed"));
                            offsetTime = System.DateTime.Now.Millisecond - Delay;

                            enemy.GetComponentInChildren<EnemyManager>().SetNearestSlime(Objects[0].gameObject);
                            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "InGameScene03")
                                EffectManager.instance.SetTargetingEffectParent(ref enemyParriedTarget);


                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_MOVE:
                        {
                            Protocol.sc_packet_move p = Protocol.sc_packet_move.SetByteToVar(data);

                           // Debug.Log("MOVE PACKET x : " + p.x);

                            int pid = ServerID_To_ClientID(p.id);
                            if (pid == -1)
                                break;
                            if (!debugStart)
                            {

                                //print("pid : " +pid);
                                if (pid < 3)   // ÇÃ·¹ÀÌ¾îÀÌ¸é
                                {
                                    Objects[pid].GetComponentInChildren<PlayerManager>().PlayerSpinDirection(p.x, p.y, p.z);
                                    Objects[pid].GetComponentInChildren<PlayerManager>().JumpTrig();
                                    Objects[pid].GetComponentInChildren<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                    Objects[pid].GetComponentInChildren<PlayerManager>().cellTag();

                                }
                                else
                                {
                                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "InGameScene03")
                                        EffectManager.instance.JumpAttack(5000, new HexCoordinates(p.x, p.z), false);
                                    else
                                    {
                                        Objects[pid].GetComponentInChildren<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                    }
                                }
                               
                            }
                            else    //debug time
                            {
                                player.GetComponentInChildren<PlayerManager>().PlayerSpinDirection(p.x, p.y, p.z);
                                player.GetComponent<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                player.GetComponent<PlayerManager>().JumpTrig();
                                Objects[pid].GetComponentInChildren<PlayerManager>().cellTag();
                            }

                            //NEAREST Slime
                            {
                                int nearestSlime = -1;
                                float nearestDistance = float.MaxValue;
                                for (int i = 0; i < 3; i++)
                                {
                                    if (Objects[i] != null)
                                    {
                                        float distance = enemy.GetComponent<HexCellPosition>().getDistance(Objects[i].GetComponent<HexCellPosition>());
                                        if (distance < nearestDistance)
                                        {
                                            nearestDistance = distance;
                                            nearestSlime = i;
                                        }
                                    }
                                }
                                if (nearestSlime != -1)
                                {
                                    //print("nearest slime : " + nearestSlime);
                                    enemy.GetComponentInChildren<EnemyManager>().SetNearestSlime(Objects[nearestSlime].gameObject);
                                    //Objects[nearestSlime].GetComponentInChildren<PlayerManager>().SetNearSlime(Objects[pid]);
                                }
                            }
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_ATTACK:
                        {
                            Protocol.sc_packet_attack p = Protocol.sc_packet_attack.SetByteToVar(data);
                            if(p.id == -1)
                            {
                                enemyAnim.SetTrigger("Hit");
                            }
                            int target_id = ServerID_To_ClientID(p.target_id);
                            if (target_id == -1)
                                break;
                            //int randomTickForTest = 4;//Random.Range(1, 6);
                            //enemy.GetComponent<EnemyManager>().BeatPatternServe(nowBeat, new Beat(0, randomTickForTest), Objects[target_id]);
                            //Objects[target_id].GetComponent<PlayerManager>().SetBallBeat(nowBeat, new Beat(0, randomTickForTest));
                            //Debug.Log("ServerID_To_ClientID : " + p.target_id+ " to " + target_id);
                            if (target_id != 3) // is not boss
                            {
                                HPManager hm = Objects[target_id].GetComponentInChildren<PlayerManager>().HP;
                                //Debug.Log("ID : " + p.target_id + "damage : " + (hm.CurrentHP - p.hp));

                                //Debug.Log("ATTACK : " + target_id + ", HP : " + hm.CurrentHP +" to " + p.hp);
                                //Debug.Log("Target : " + target_id);
                                if (target_id == myPlayerID)
                                {
                                    if (hm.CurrentHP - p.hp > 0)
                                    {
                                        resultsData.damaged += (hm.CurrentHP - p.hp);

                                        ComboEffect.CountApply(ref nowCombo, true);
                                        CineCameraShake.instance.ShakeCamera(hm.CurrentHP - p.hp);
                                        // µ¥¹ÌÁö ¶ç¿ì±â
                                        ComboEffect.DamageApply(hm.CurrentHP - p.hp);
                                        //VFXManager.data.HitSounder((hm.CurrentHP - p.hp) / 30f);
                                    }
                                }

                                Objects[target_id].GetComponentInChildren<PlayerManager>().StunTrig();
                                hm.Damage(hm.CurrentHP - p.hp);


                            }
                            else
                            {
                                //Objects[ServerID_To_ClientID(p.id)].GetComponentInChildren<PlayerManager>().AttackTrig();

                                HPManager hm = Objects[target_id].GetComponentInChildren<EnemyManager>().HP;

                                if (hm.CurrentHP - p.hp > 0)
                                {
                                    resultsData.attack += (hm.CurrentHP - p.hp);

                                    hm.Damage(hm.CurrentHP - p.hp);
                                    ComboEffect.AttackApply(hm.CurrentHP - p.hp);
                                }

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
                                Objects[pid].GetComponentInChildren<PlayerManager>().playerClassofSkill = p.skillType;
                                Objects[pid].GetComponentInChildren<PlayerManager>().playerLevelofSkill = p.skillLevel;
                                Debug.Log(pid + ": " + p.skillType + ", " + p.skillLevel);

                                HPGM.PlayerHPs[0].Set(p.skillType, FieldPlayerManager.myName);

                                break;
                            }
                            //PutObject(p.type, p.id, p.x, p.y);
                            switch (p.obj_type)
                            {
                                case (byte)Protocol.OBJECT_TYPE.PLAPER:
                                    {
                                        //print("PLAPER");
                                        if (HPGMStaticInt <= 2)
                                        {
                                            // Debug.Log(p.id + ", " + p.x + ", " + p.y + ", " + p.z + ", " + "ï¿½Ã·ï¿½ï¿½Ì¾ï¿½ ï¿½ï¿½ï¿½ï¿½");
                                            Objects[pid] = ObjectPool.instance.PlayerObjectQueue.Dequeue();
                                            Objects[pid].SetActive(true);
                                            Objects[pid].GetComponentInChildren<PlayerManager>().SetHPImages(HPGM.PlayerHPs[HPGMStaticInt].HPImage, HPGM.PlayerHPs[HPGMStaticInt].prevHPImage);
                                            Objects[pid].GetComponentInChildren<PlayerManager>().playerClassofSkill = p.skillType;
                                            Objects[pid].GetComponentInChildren<PlayerManager>().playerLevelofSkill = p.skillLevel;
                                            //Debug.Log(pid + ": " + p.skillType + ", " + p.skillLevel);
                                            HPGM.PlayerHPs[HPGMStaticInt].Set(p.skillType, System.Text.Encoding.UTF8.GetString(p.name));
                                            HPGMStaticInt++;


                                            Objects[pid].GetComponentInChildren<Animator>().SetFloat("Speed", bpm / 45.0f);

                                            Objects[pid].GetComponentInChildren<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                            Objects[pid].GetComponentInChildren<HexCellPosition>().setDirection(p.direction);
                                        }
                                        break;
                                    }
                                case (byte)Protocol.OBJECT_TYPE.ENEMY:
                                    {
                                        // Debug.Log(p.id + ", " + p.x + ", " + p.y + ", " + p.z + ", " + "ï¿½Ã·ï¿½ï¿½Ì¾ï¿½ ï¿½ï¿½ï¿½ï¿½");
                                        Objects[pid] = enemy;
                                        Objects[pid].SetActive(true);
                                        //enemyAnim.SetFloat("Speed", bpm / 45.0f);

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
                            else
                                break;
                            switch (p.effect_type)
                            {
                                case 55:// skill
                                    // p->x  == skill Level
                                    // p->y  == skill type
                                    switch (p.y)
                                    {
                                        case 1:
                                            EffectManager.instance.PlayerWaterGunEffect(Objects[pid].transform.localPosition, ref Objects[tid], p.charging_time);
                                            HPGM.PlayerHPs[pid].nowCooltime = HPGM.PlayerHPs[pid].cooltime;
                                            break;
                                        case 2:
                                            {
                                                HexCoordinates cell = Objects[pid].GetComponent<HexCellPosition>().coordinates;
                                                EffectManager.instance.PlayerQuakeEffect(cell.X, cell.Y, cell.Z, p.charging_time);
                                                HPGM.PlayerHPs[pid].nowCooltime = HPGM.PlayerHPs[pid].cooltime;
                                                //Debug.Log(cell.X + ", " + cell.Y + ", " + cell.Z + " skill attack");

                                            }
                                            break;
                                        case 3:
                                            {
                                                HexCoordinates cell = Objects[pid].GetComponent<HexCellPosition>().coordinates;
                                                EffectManager.instance.PlayerHealEffect(cell.X, cell.Y, cell.Z, p.charging_time);
                                                HPGM.PlayerHPs[pid].nowCooltime = HPGM.PlayerHPs[pid].cooltime;
                                                //Debug.Log(cell.X + ", " + cell.Y + ", " + cell.Z + " skill attack");

                                            }
                                            break;
                                    }
                                    // Debug.Log(p.y + " skill type");
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
                            //p.item_type 0~20 Áß ÇÏ³ª
                            //¸¶³à¸ÊÀÇ °æ¿ì 0~9
                            //½ºÆÀÆãÅ© °æ¿ì 10~20
                            if (p.end_type == 0)
                            {
                                gameOverImage.SetGameEnd(GameEndTraits.Lose);
                                gameOverImage.SetResultData(resultsData.perfect, resultsData.great, resultsData.miss, resultsData.attack, resultsData.damaged, 0, 0, 0);
                                Debug.Log("Game_Over");
                            }
                            else if (p.end_type == 1)
                            {
                                gameOverImage.SetGameEnd(GameEndTraits.Win);
                                gameOverImage.SetResultData(resultsData.perfect, resultsData.great, resultsData.miss, resultsData.attack, resultsData.damaged,
                                                            p.score, p.money, p.item_type);

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

                            if (pid == myPlayerID)
                            {
                                MidANote.notePerfect();
                                JudgeEffect.GetComponent<IndicatorJudgeEffect>().JudgeApply(JudgeCases.PERFECT);
                                ParticleEffect.ParticleApply(JudgeCases.PERFECT);
                                ComboEffect.CountApply(ref nowCombo);
                            }
                            EffectManager.instance.BossRokets.GetComponent<TargetingEffect>().ParryingNearRoket(Objects[pid].transform);

                            //Debug.Log("Parrying Success");
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PING_TEST:
                        {
                            Protocol.sc_packet_ping_test p = Protocol.sc_packet_ping_test.SetByteToVar(data);

                            int ping = System.DateTime.Now.Millisecond - p.ping_time;
                            Network.ping_data[Network.ping_index++] = ping;
                            Network.ping_index = Network.ping_index % Network.ping_data.Length;
                            avgPing = Network.GetPingAvg();
                            //Debug.Log("avgPing : " + avgPing);
                            //Debug.Log(ping);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_SEND_SCORE:
                        {
                            Protocol.sc_packet_score p = Protocol.sc_packet_score.SetByteToVar(data);
                            //p.id => Á¡¼ö°¡ ¹Ù²ï ÇÃ·¹ÀÌ¾î ¾ÆÀÌµð
                            //p.score => Á¡¼ö
                            if (p.id == myPlayerID)
                                ComboEffect.ScoreApply(p.score);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_USE_ITEM:
                        {
                            Protocol.sc_packet_use_item p = Protocol.sc_packet_use_item.SetByteToVar(data);
                            //p.user -> »ç¿ëÀÚ
                            //p.itemType -> »ç¿ë ¾ÆÀÌÅÛ 
                            
                        }
                        break;
                    default:
                        Debug.Log("wrong + type : " + type);

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

                if (beatCounter <= JudgementTiming / 2f)
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

                if ((timeByBeat - beatCounter) <= JudgementTiming / 2f)
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
                // Debug.Log("Error Beside : -" + beatCounter + ", +" + (timeByBeat - beatCounter));
                resultsData.miss++;
                JudgeEffect.GetComponent<IndicatorJudgeEffect>().JudgeApply(JudgeCases.BAD);
                //ParticleEffect.ParticleApply(JudgeCases.BAD);
                ComboEffect.CountApply(ref nowCombo, true);
                return 0;   //ï¿½ß¸ï¿½ ï¿½ï¿½ï¿½ï¿½
            }
        }
        else
        {
            //ComboEffect.CountApply(ref nowCombo, true);
            //MidNote.noteEnd();
            //Debug.Log("Cutting");
        }
        return 0;
    }

    public void setMoved()
    {
        if (beatCounter <= JudgementTiming)
        {
            //Debug.Log("Judge : -" + beatCounter);

        }
        else if (timeByBeat - beatCounter <= JudgementTiming)
        {
            //Debug.Log("Judge : +" + (timeByBeat - beatCounter));
        }

        alreadyMoved = JudgementTiming * 2;
    }

}
