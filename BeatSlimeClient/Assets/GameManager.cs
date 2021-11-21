using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager data;
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
    [SerializeField] Beat nowBeat;
    public int beatCounter = 0;

    public int offsetTime;

    public int JudgementTiming;

    public int alreadyMoved;

    //
    public Network Net = new Network();
    GameObject[] Players = new GameObject[Protocol.CONSTANTS.MAX_CLIENT];
    GameObject[] Enemys = new GameObject[4];// 수정
    int myPlayerID = -1;
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
        if (!isGameStart)
        {
            PlaySound();
            offsetTime = 0;
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
        else
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
                    Players[myPlayerID].GetComponent<PlayerManager>().Beat();
                    enemy.GetComponent<EnemyManager>().Beat();
                    grid.Beat();
                    soundEffectManager.BeatEffect();
                    beatCounter = timeByBeat;
                }
            }

            if (alreadyMoved > 0)
                alreadyMoved -= (int)(Time.deltaTime * 10000f);

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
                            Players[p.id] = player;
                            //PutPlayerObject(p.type, p.id, p.x, p.y);
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
                            Debug.Log("이동");
                            //MoveObject(p.type, p.id, p.x, p.y);
                            Debug.Log(p.x + "," + p.y + ", " + p.z);

                            //Debug.Log(p.id+"이동");
                            Players[p.id].GetComponent<HexCellPosition>().SetPosition(p.x, p.y, p.z);

                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_MOVE_OBJECT:
                        {
                            Protocol.sc_packet_move_object p = Protocol.sc_packet_move_object.SetByteToVar(data);
                            //Debug.Log("이동");
                            //MoveObject(p.type, p.id, p.x, p.y);
                            //Debug.Log(p.x + "," + p.y + ", " + p.z);

                            //Debug.Log(p.id + "이동");
                            Enemys[p.id].GetComponent<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                            Enemys[p.id].GetComponent<HexCellPosition>().setDirection(p.direction);
                            //Debug.Log(p.id + ", " + p.x + ", " + p.y + ", " + p.z + ", " + "마녀 넣음");

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
                                        Debug.Log(p.id + ", " + p.x + ", " + p.y + ", " + p.z + ", " + "플레이어 넣음");
                                        Players[p.id] = ObjectPool.instance.PlayerObjectQueue.Dequeue();
                                        Players[p.id].SetActive(true);
                                        Players[p.id].GetComponentInChildren<Animator>().SetFloat("Speed", bpm / 45.0f);

                                        Players[p.id].GetComponent<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                        break;
                                    }
                                case (byte)Protocol.OBJECT_TYPE.ENEMY:
                                    {
                                        Debug.Log(p.id + ", " + p.x + ", " + p.y + ", " + p.z + ", " + "마녀 넣음");

                                        Enemys[p.id] = ObjectPool.instance.EnemyObjectQueue.Dequeue();
                                        Enemys[p.id].SetActive(true);
                                        Enemys[p.id].GetComponent<HexCellPosition>().SetPosition(p.x, p.y, p.z);
                                    }
                                    break;
                            }

                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_REMOVE_OBJECT:
                        {
                            Protocol.sc_packet_remove_object p = Protocol.sc_packet_remove_object.SetByteToVar(data);
                            ObjectPool.instance.EnemyObjectQueue.Enqueue(Players[p.id]);
                            Players[p.id].SetActive(false);
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
                return 1;  //일찍 누름
            }
            else if (timeByBeat - beatCounter <= JudgementTiming)
            {
                return 1;   //늦게 누름
            }
            else
            {
                Debug.Log("Error Beside : -" + beatCounter + ", +" + (timeByBeat - beatCounter));
                return 0;   //잘못 누름
            }
        }
        else
        {
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
