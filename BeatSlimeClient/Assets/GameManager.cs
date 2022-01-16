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

    public SoundManager soundManager;
    public SoundEffectManager soundEffectManager;
    public SoundProgressBar soundProgressBar;

    public BeatManager beatManager;

    public bool isGameStart;
    public bool bGameStart;

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
    public Network Net = new Network();
    GameObject[] Objects = new GameObject[Protocol.CONSTANTS.MAX_OBJECT];

    int myPlayerID = -1;
    public ArrayList Mapdata = new ArrayList();

    void Awake()
    {
        print("Start");
        data = this;
        isGameStart = false;
        alreadyMoved = 0;

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
        Net.CloseSocket();
    }
    void Start()
    {
        //DEBUG
        enemy.GetComponent<HexCellPosition>().setInitPosition(1, 1);
        PM = PatternManager.data;

        Net.CreateAndConnect();
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown("1"))
        {
            if (Mapdata.Count != 0)
                Net.SendWriteMapPacket(Mapdata);
        }
        else if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            Net.SendreadPacket();
        }

        //if (Input.GetKeyDown("z"))
        //{
        //    if (!isGameStart)
        //    {
        //        PlaySound();
        //        //offsetTime = 0;
        //    }
        //    else
        //    {
        //        int randomTickForTest = Random.Range(1, 6);
        //        enemy.GetComponent<EnemyManager>().BeatPatternServe(nowBeat, new Beat(0, randomTickForTest), player);
        //        player.GetComponent<PlayerManager>().SetBallBeat(nowBeat, new Beat(0, randomTickForTest));
        //    }
        //}

        if (bGameStart)
        {
            PlaySound();
        }

        if (isGameStart && !Net.isOnline)
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
        else if (isGameStart && Net.isOnline)
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
        if (Net.isOnline)
        {
            // 네트워크 메세지 큐
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

                            PlaySound();
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_MOVE:
                        {
                            Protocol.sc_packet_move p = Protocol.sc_packet_move.SetByteToVar(data);


                            //Debug.Log(p.id+"이동");
                            Objects[p.id].GetComponent<HexCellPosition>().setDirection((byte)p.dir);
                            Objects[p.id].GetComponent<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                            if (p.id < Protocol.CONSTANTS.MAX_USER)
                                Objects[p.id].GetComponent<PlayerManager>().JumpTrig();
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_ATTACK:
                        {
                            Protocol.sc_packet_attack p = Protocol.sc_packet_attack.SetByteToVar(data);
                            //Debug.Log(p.id+"가 " + p.target_id + "를 공격");
                            //MoveObject(p.type, p.id, p.x, p.y);
                            //Debug.Log(p.x + "," + p.y + ", " + p.z);

                            //Debug.Log(p.id+"이동");
                            //Objects[p.id].GetComponent<HexCellPosition>().setDirection(p.direction);

                            int randomTickForTest = 4;//Random.Range(1, 6);
                            enemy.GetComponent<EnemyManager>().BeatPatternServe(nowBeat, new Beat(0, randomTickForTest), Objects[p.target_id]);
                            Objects[p.target_id].GetComponent<PlayerManager>().SetBallBeat(nowBeat, new Beat(0, randomTickForTest));
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PUT_OBJECT:
                        {
                            Protocol.sc_packet_put_object p = Protocol.sc_packet_put_object.SetByteToVar(data);

                            //PutObject(p.type, p.id, p.x, p.y);
                            switch (p.obj_type)
                            {
                                case (byte)Protocol.OBJECT_TYPE.PLAPER:
                                    {
                                        // Debug.Log(p.id + ", " + p.x + ", " + p.y + ", " + p.z + ", " + "플레이어 넣음");
                                        Objects[p.id] = ObjectPool.instance.PlayerObjectQueue.Dequeue();
                                        Objects[p.id].SetActive(true);
                                        Objects[p.id].GetComponentInChildren<Animator>().SetFloat("Speed", bpm / 45.0f);

                                        Objects[p.id].GetComponent<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                        break;
                                    }
                                case (byte)Protocol.OBJECT_TYPE.ENEMY:
                                    {
                                        // Debug.Log(p.id + ", " + p.x + ", " + p.y + ", " + p.z + ", " + "마녀 넣음");

                                        //Objects[p.id] = ObjectPool.instance.EnemyObjectQueue.Dequeue();
                                        Objects[p.id] = enemy;
                                        Objects[p.id].SetActive(true);
                                        Objects[p.id].GetComponent<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                    }
                                    break;
                            }

                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_REMOVE_OBJECT:
                        {
                            Protocol.sc_packet_remove_object p = Protocol.sc_packet_remove_object.SetByteToVar(data);
                            if (p.id < Protocol.CONSTANTS.MAX_USER)
                            {
                                ObjectPool.instance.PlayerObjectQueue.Enqueue(Objects[p.id]);
                                Objects[p.id].SetActive(false);
                            }
                            else
                            {
                                ObjectPool.instance.EnemyObjectQueue.Enqueue(Objects[p.id]);
                                Objects[p.id].SetActive(false);
                            }

                            //다른 플레이어면 다른플레이어 풀에
                            //적이면 적풀에 넣자
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

                            //다른 플레이어면 다른플레이어 풀에
                            //적이면 적풀에 넣자
                            //ReMoveObject(p.id);
                        }
                        break;
                    default:
                        Debug.Log("이상한 타입이네");
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

                //DEBUG : 패링 비트에는 아래 코드를
                //MidANote.notePerfect();
                return 1;  //일찍 누름
            }
            else if (timeByBeat - beatCounter <= JudgementTiming)
            {
                MidNote.notePerfect();
                return 1;   //늦게 누름
            }
            else
            {
                MidNote.noteEnd();
                Debug.Log("Error Beside : -" + beatCounter + ", +" + (timeByBeat - beatCounter));
                return 0;   //잘못 누름
            }
        }
        else
        {
            MidNote.noteEnd();
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
