using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FieldGameManager : MonoBehaviour
{
    public static FieldGameManager data;

    public int mapCellid = 0;

    public GameObject player;
    public GameObject tilemap;
    public FieldHexGrid grid;

    public MapLoader loader;

    public SoundManager soundManager;
    public SoundEffectManager soundEffectManager;

    public bool isGameStart;

    //
    public static Network Net = new Network();
    static GameObject[] Objects = new GameObject[Protocol.CONSTANTS.MAX_OBJECT];

    static int myPlayerID = -1;
    public ArrayList Mapdata = new ArrayList();

    void Awake()
    {
        print("Start");
        data = this;
        isGameStart = false;

        if (grid.TMP)
        {
            loader.Match(grid);
            loader.LoadMap();
        }

        Net.SendChangeSceneDonePacket(0);
        if (myPlayerID != -1)
        {
            Objects[myPlayerID] = player;
        }
    }
    private void OnApplicationQuit()
    {
        Net.CloseSocket();
    }
    void Start()
    {
        if (false == Net.isOnline)
            Net.CreateAndConnect();

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            soundManager.StopBGM();
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            //id를 정해서 SendPartyRequestPacket의 인자로 전달
            //파티를 하고 싶은 플레이어를 정해서 id를 인자로 하고 아래 함수를 호출하면됨(클라와 서버는 같은 id를 쓰고 있음)
            Net.SendPartyRequestPacket(0);


            //파티 요청이 오면 수락 여부를 알려줌 (0 거절, 1 수락)
            // 두 번째인자는 myPlayerID를 넣어주면됨 
            Net.SendPartyRequestAnwserPacket(0, myPlayerID);

            //Protocol.CONSTANTS.SC_PACKET_PARTY_REQUEST: -> 파티 요청이 왔다.
            //Protocol.CONSTANTS.SC_PACKET_PARTY_REQUEST_ANWSER: -> 파티 요청에 대한 수락 여부가 왔다.
        }
        // 상대방의 요청 수락 여부가 SC패킷으로 옴

        if (Net.isOnline)
        {
            isGameStart = true;
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

                            soundManager.PlayBGM("riverside");
                            player.GetComponentInChildren<Animator>().SetFloat("Speed", PlayerPrefs.GetFloat("pAnimSpeed"));
                            myPlayerID = p.id;
                            Objects[p.id] = player;
                            //PutPlayerObject(p.type, p.id, p.x, p.y);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_CHANGE_SCENE:
                        {
                            Protocol.sc_packet_change_scene p = Protocol.sc_packet_change_scene.SetByteToVar(data);
                            SceneManager.LoadScene(p.scene_num);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_MOVE:
                        {
                            Protocol.sc_packet_move p = Protocol.sc_packet_move.SetByteToVar(data);

                            //Debug.Log(p.id+"이동");
                            //Debug.Log((byte)p.dir);
                            Objects[p.id].GetComponent<FieldHexCellPosition>().setDirection((byte)p.dir);
                            Objects[p.id].GetComponent<FieldHexCellPosition>().SetPosition(p.x, p.y, p.z);
                            if (p.id < Protocol.CONSTANTS.MAX_USER)
                                Objects[p.id].GetComponent<FieldPlayerManager>().JumpTrig();
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
                                        Objects[p.id].GetComponentInChildren<Animator>().SetFloat("Speed", 120 / 45.0f);

                                        Objects[p.id].GetComponentInChildren<FieldHexCellPosition>().SetPosition(p.x, p.y, p.z);
                                        break;
                                    }
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
                    case Protocol.CONSTANTS.SC_PACKET_CHANGE_SKILL:
                        {
                            Protocol.sc_packet_change_skill p = Protocol.sc_packet_change_skill.SetByteToVar(data);
                            Debug.Log(p.id + "가 " + p.skill_type + "으로 스킬을 바꿈");
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PARTY_REQUEST:
                        {
                            //요청이 왔을 때 수락, 거부
                            //이미 파티가 있다면 이 패킷이 오지 않음

                            //수락
                            Net.SendPartyRequestAnwserPacket(1, myPlayerID);
                            //거절
                            Net.SendPartyRequestAnwserPacket(0, myPlayerID);

                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PARTY_REQUEST_ANWSER:
                        {
                            //내가 보낸 요청이 거절 됐는지 수락 됐는지 알려주는 패킷
                            Protocol.sc_packet_party_request_anwser p = Protocol.sc_packet_party_request_anwser.SetByteToVar(data);

                            Debug.Log(p.p_id + "가 " + p.anwser + " 이라고 응답함");

                        }
                        break;
                    default:
                        Debug.Log("이상한 타입이네");
                        break;
                }
            }
        }
    }
}
