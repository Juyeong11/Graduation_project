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
    public ChattingManager chattingManager;
    public bool isGameStart;

    //
    public static Network Net = new Network();
    static GameObject[] Objects = new GameObject[Protocol.CONSTANTS.MAX_OBJECT];

    public static int myPlayerID = -1;
    public ArrayList Mapdata = new ArrayList();

    int scene_num;

    public GameObject ResponseMenu;
    public MusicName MN;

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


        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            scene_num = 2;
            StartCoroutine(ChangeScene());
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            MN.ChangeMusicName("flower load - zeroste.");
        }

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
                            MN.ChangeMusicName("riverside - zeroste.");
                            player.GetComponentInChildren<Animator>().SetFloat("Speed", PlayerPrefs.GetFloat("pAnimSpeed"));
                            myPlayerID = p.id;
                            Objects[p.id] = player;
                            //PutPlayerObject(p.type, p.id, p.x, p.y);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_CHANGE_SCENE:
                        {
                            Protocol.sc_packet_change_scene p = Protocol.sc_packet_change_scene.SetByteToVar(data);

                            scene_num = p.scene_num;
                            StartCoroutine(ChangeScene());
                            
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_MOVE:
                        {
                            Protocol.sc_packet_move p = Protocol.sc_packet_move.SetByteToVar(data);
                            

                            //Debug.Log(p.id+"이동");
                            //Debug.Log((byte)p.dir);

                            //grid.cellMaps.Get(p.x, p.y, p.z).obejct.GetComponent<HexCellPosition>().enableToMove_ForField = false;
                            grid.cellMaps.Get(p.x, p.y, p.z).obejct.GetComponentInChildren<SpriteRenderer>().enabled = false;
                            if (p.id < Protocol.CONSTANTS.MAX_USER)
                            {
                                if (p.id == myPlayerID)
                                {
                                    Objects[p.id].GetComponentInParent<FieldPlayerManager>().PlayerSpinDirection(p.x, p.y, p.z);
                                    Objects[p.id].GetComponentInParent<FieldPlayerManager>().JumpTrig();
                                }
                                else
                                {
                                    Objects[p.id].GetComponent<FieldOtherPlayerManager>().PlayerSpinDirection(p.x, p.y, p.z);
                                    Objects[p.id].GetComponent<FieldOtherPlayerManager>().JumpTrig();

                                }
                            }
                            Objects[p.id].GetComponent<FieldHexCellPosition>().SetPosition(p.x, p.y, p.z);
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
                                        Objects[p.id].GetComponentInChildren<FieldHexCellPosition>().direction = (HexDirection)p.direction;
                                        //Debug.Log(p.id + " 플레이어 넣음");
                                        Objects[p.id].GetComponent<FieldOtherPlayerManager>().pID = p.id;

                                        //Objects[p.id].GetComponent<FieldOtherPlayerManager>().other_skillnum = p.skill_type;
                                        //grid.cellMaps.Get(p.x, p.y, p.z).obejct.GetComponent<HexCellPosition>().enableToMove_ForField = false;
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

                            if(p.id == myPlayerID)
                                Objects[p.id].GetComponentInParent<FieldPlayerManager>().ChangeSkill(p.skill_type);
                            else
                            {
                                Objects[p.id].GetComponent<FieldOtherPlayerManager>().ChangeSkill(p.skill_type);
                                PartyManager.instance.PartyChangeClass(p.id, p.skill_type);
                            }
                            Debug.Log(p.id + "가 " + p.skill_type + "으로 스킬을 바꿈");
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PARTY_REQUEST:
                        {
                            //요청이 왔을 때 수락, 거부
                            //이미 파티가 있다면 이 패킷이 오지 않음
                            Protocol.sc_packet_party_request p = Protocol.sc_packet_party_request.SetByteToVar(data);

                            ResponseMenu.transform.position = player.transform.position;
                            ResponseMenu.GetComponent<ResponseBillboardUI>().GetOn(player.transform, p.requester_id);

                            //수락
                            
                            //Net.SendPartyRequestAnwserPacket(1, p.requester_id);
                            //거절
                            //Net.SendPartyRequestAnwserPacket(0, p.requester_id);

                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_PARTY_REQUEST_ANWSER:
                        {
                            //내가 보낸 요청이 거절 됐는지 수락 됐는지 알려주는 패킷
                            Protocol.sc_packet_party_request_anwser p = Protocol.sc_packet_party_request_anwser.SetByteToVar(data);

                            Debug.Log(p.p_id + "가 " + p.anwser + " 이라고 응답함");

                            /*
                             * 0 요청 거절
                             * 1 수락
                             * 2 내가 수락한 파티의 인원이 이미 가득 찾을 경우 -> 파티 신청을 여러명에게 보낸경우
                             * 3 다른 사람이 파티에서 탈퇴한경우
                             *  - CS_PACKET_PARTY_REQUEST_ANWSER패킷을 서버에 보낼 때  requester id가 -1이면 파티에 탈퇴하려고 하는 걸로 알고 서버에서 처리함
                             */
                            switch (p.anwser)
                            {
                                case 0:
                                chattingManager.SetMess("<color=red>상대가 파티 신청을 거절했습니다!</color>");
                                break;

                                case 1:
                                chattingManager.SetMess("<color=red>" + p.p_id + "님이 새로운 파티원이 되었습니다!</color>");
                                PartyManager.instance.SetParty(p.p_id, Objects[p.p_id].GetComponent<FieldOtherPlayerManager>().other_skillnum);
                                break;

                                case 2:
                                chattingManager.SetMess("<color=red>상대의 파티에 남은 자리가 없습니다!</color>");
                                break;

                                case 3:
                                chattingManager.SetMess("<color=red>" + p.p_id + "님이 파티에서 탈퇴하셨습니다.</color>");
                                PartyManager.instance.DelParty(p.p_id);
                                break;

                                case 4:
                                    chattingManager.SetMess("<color=red>상대는 이미 파티가 있습니다!</color>");
                                    break;
                            }
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_CHAT:
                        {
                            Protocol.sc_packet_chat p = Protocol.sc_packet_chat.SetByteToVar(data);

                            string mess = System.Text.Encoding.UTF8.GetString(p.mess).Split('\0')[0];
                            //Debug.Log(System.Text.Encoding.UTF8.GetString(p.mess));
                            chattingManager.SetMess(p.p_id+ ": " + mess);
                        }
                        break;
                    default:
                        Debug.Log("이상한 타입이네");
                        break;
                }
            }
        }
    }

    IEnumerator ChangeScene()
    {
        player.GetComponentInParent<FieldPlayerManager>().PortalPlane.transform.SetParent(null);
        //player.GetComponent<FieldPlayerManager>().PortalPlane.transform.localRotation = Quaternion.Euler(90,0,0);
        player.GetComponentInParent<FieldPlayerManager>().PortalPlane.SetActive(true);
        
        yield return new WaitForSeconds(2.0f);
        player.GetComponentInParent<FieldPlayerManager>().EnterPortal();
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(scene_num);
    }
}
