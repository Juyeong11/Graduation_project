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
    //
    GameObject[] Objects = new GameObject[4];
    int[] ids;
    int myPlayerID = -1;
    public ArrayList Mapdata = new ArrayList();

    void Awake()
    {
        print("Start");
        data = this;
        isGameStart = false;
        alreadyMoved = 0;

        loader.Match(grid);
        loader.LoadMap();

        nowBeat = new Beat();

        bpm = SoundManager.instance.GetBGMBpm(SongName);
        totalSongTime = (int)(SoundManager.instance.GetMusicLength(SongName) * 1000f);
        nowSongTime = 0;
        timeByBeat = (int)(1000f * 60f / bpm);
        timeByBar = timeByBeat * barCounts;
        timeBy24Beat = timeByBeat / 6;
        timeBy16Beat = timeByBeat / 4;
    }
    private void OnApplicationQuit()
    {
        FieldGameManager.Net.CloseSocket();
    }
    void Start()
    {

        if (!FieldGameManager.Net.isServerOnline()) FieldGameManager.Net.CreateAndConnect();

        loader.LoadMap();

        //DEBUG
        enemy.GetComponent<HexCellPosition>().setInitPosition(1, 1);
        PM = PatternManager.data;
        ids = new int[4];
    }

    void FixedUpdate()
    {

        //if (Input.GetKeyDown("1"))
        //{
        //if (Mapdata.Count != 0)
        //FieldGameManager.Net.SendWriteMapPacket(Mapdata);
        //}
        //else if (Input.GetKeyDown(KeyCode.BackQuote))
        //{
        //    FieldGameManager.Net.SendreadPacket();
        //}
        if (Input.GetKeyDown(KeyCode.Return))
        {
            FieldGameManager.Net.SendGameStartReadyPacket();
        }


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
                    Objects[myPlayerID].GetComponent<PlayerManager>().Beat();
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
            // ��Ʈ��ũ �޼��� ť
            if (Network.MessQueue.Count > 0)
            {
                byte[] data = Network.MessQueue.Dequeue();

                byte type = data[1];

                switch (type)
                {
                    case Protocol.CONSTANTS.SC_PACKET_LOGIN_OK:
                        {
                            Protocol.sc_packet_login_ok p = Protocol.sc_packet_login_ok.SetByteToVar(data);

                            myPlayerID = p.id;
                            Objects[p.id] = player;
                            //PutPlayerObject(p.type, p.id, p.x, p.y);
                            PlaySound();
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_GAME_START:
                        {
                            Protocol.sc_packet_game_start p = Protocol.sc_packet_game_start.SetByteToVar(data);
                            Debug.Log("GS PACKET x : " + p.boss_id);
                            ids[0] = p.id1;
                            ids[1] = p.id2;
                            ids[2] = p.id3;
                            ids[3] = p.boss_id;
                            int pid = ServerID_To_ClientID(p.player_id);
                            Objects[pid] = player;
                            myPlayerID = pid;
                            Objects[3] = enemy;
                            Objects[3].SetActive(true);

                            PlaySound();
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_MOVE:
                        {
                            Protocol.sc_packet_move p = Protocol.sc_packet_move.SetByteToVar(data);

                            Debug.Log("MOVE PACKET x : " + p.x);

                            int pid = ServerID_To_ClientID(p.id);
                            //Debug.Log(p.id+"�̵�");
                            Objects[pid].GetComponent<HexCellPosition>().setDirection((byte)p.dir);
                            Objects[pid].GetComponent<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                            if (pid == myPlayerID)// �÷��̾� �̸�
                                Objects[pid].GetComponent<PlayerManager>().JumpTrig();
                            else if (pid < 3)
                                Objects[pid].GetComponent<InGameOtherPlayerManager>().JumpTrig();


                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_ATTACK:
                        {
                            Protocol.sc_packet_attack p = Protocol.sc_packet_attack.SetByteToVar(data);
                            //Debug.Log(p.id+"�� " + p.target_id + "�� ����");
                            //MoveObject(p.type, p.id, p.x, p.y);
                            //Debug.Log(p.x + "," + p.y + ", " + p.z);

                            //Debug.Log(p.id+"�̵�");
                            //Objects[p.id].GetComponent<HexCellPosition>().setDirection(p.direction);

                            int target_id = ServerID_To_ClientID(p.target_id);
                            int randomTickForTest = 4;//Random.Range(1, 6);
                            enemy.GetComponent<EnemyManager>().BeatPatternServe(nowBeat, new Beat(0, randomTickForTest), Objects[target_id]);
                            Objects[target_id].GetComponent<PlayerManager>().SetBallBeat(nowBeat, new Beat(0, randomTickForTest));
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PUT_OBJECT:
                        {
                            Protocol.sc_packet_put_object p = Protocol.sc_packet_put_object.SetByteToVar(data);
                            int pid = ServerID_To_ClientID(p.id);
                            if (pid == myPlayerID)
                            {
                                Objects[pid].GetComponent<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                break;
                            }
                            //PutObject(p.type, p.id, p.x, p.y);
                            switch (p.obj_type)
                            {
                                case (byte)Protocol.OBJECT_TYPE.PLAPER:
                                    {
                                        // Debug.Log(p.id + ", " + p.x + ", " + p.y + ", " + p.z + ", " + "�÷��̾� ����");
                                        Objects[pid] = ObjectPool.instance.PlayerObjectQueue.Dequeue();
                                        Objects[pid].SetActive(true);
                                        Objects[pid].GetComponentInChildren<Animator>().SetFloat("Speed", bpm / 45.0f);

                                        Objects[pid].GetComponent<HexCellPosition>().SetPosition(p.x, p.y, p.z);
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

                            //�ٸ� �÷��̾��?�ٸ��÷��̾� Ǯ��
                            //���̸� ��Ǯ�� ����
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

                            //�ٸ� �÷��̾��?�ٸ��÷��̾� Ǯ��
                            //���̸� ��Ǯ�� ����
                            //ReMoveObject(p.id);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_EFFECT:
                        {
                            Protocol.sc_packet_effect p = Protocol.sc_packet_effect.SetByteToVar(data);
                            int pid = ServerID_To_ClientID(p.id);
                            int tid = ServerID_To_ClientID(p.target_id);
                            int start_x, start_y, start_z;
                            if (p.target_id == -1)
                            {
                                start_x = p.x;
                                start_y = p.y;
                                start_z = p.z;
                            }
                            else
                            {
                                start_x = Objects[tid].GetComponent<HexCellPosition>().coordinates.X;
                                start_y = Objects[tid].GetComponent<HexCellPosition>().coordinates.Y;
                                start_z = Objects[tid].GetComponent<HexCellPosition>().coordinates.Z;
                            }
                            int start_w = 0;//Objects[tid].GetComponent<HexCellPosition>().coordinates.W;
                            switch (p.effect_type)
                            {
                                case 3:
                                    EffectManager.instance.BossTileEffect3(start_x, start_y, start_z, start_w, p.charging_time);
                                    break;
                                case 4:
                                    EffectManager.instance.BossTileEffect4(start_x, start_y, start_z, start_w, p.charging_time);
                                    break;
                                case 99:
                                    EffectManager.instance.OneTileEffect(start_x, start_y, start_z, start_w, p.charging_time);
                                    break;
                                case 5:
                                    break;
                            }
                            //StartCoroutine(EffectManager.instance.TileEffect0(0, 0, 0, 0,HexDirection.LeftDown));

                            //Debug.Log(start_x+ " " + start_y + " " + start_z + " " + start_w);
                            //EffectManager.instance.BossTileEffect1(0, 0, 0, 0);
                        }
                        break;
                    default:
                        Debug.Log("�̻��� Ÿ���̳�" + type);
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
        Debug.Log("���� ID�Դϴ�");
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

                //DEBUG : �и� ��Ʈ���� �Ʒ� �ڵ带
                //MidANote.notePerfect();
                return 1;  //���� ����
            }
            else if (timeByBeat - beatCounter <= JudgementTiming)
            {
                MidNote.notePerfect();
                return 1;   //�ʰ� ����
            }
            else
            {
                //MidNote.noteEnd();
                Debug.Log("Error Beside : -" + beatCounter + ", +" + (timeByBeat - beatCounter));
                return 0;   //�߸� ����
            }
        }
        else
        {
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
