using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FieldGameManager : MonoBehaviour
{
    public static FieldGameManager data;

    public int mapCellid = 0;

    public GameObject player;
    public Animator playerAnim;
    public Animator outlineAnim;
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
    public ShopPrices shopPrices;

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
            MN.ChangeMusicName(soundManager.getSongName());
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Net.SendTeleportPacket(0); // 1번 포탈로 이동
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            Net.SendTeleportPacket(1); // 100골드 획득
            PlayerPrefs.SetInt("Money" + myPlayerID, PlayerPrefs.GetInt("Money" + myPlayerID, 0) + 100);
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            //아이템           가격      damage    cooltime
            // 0 : AD0         0        5         10
            // 1 : AD1         50       10        8
            // 2 : AD2         150      20        7
            // 3 : AD3         500      40        6
            // 4 : Heal0       0        5         10
            // 5 : Heal1       50       5         8
            // 6 : Heal2       200      10        7
            // 7 : Heal3       800      20        6
            // 8 : Tank0       0        10        10
            // 9 : Tank1       50       20        8
            // 10: Tank2       150      40        7
            // 11: Tank3       500      80        6
            Net.SendBuyPacket(0); // 0번 아이템 구매
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            //scrool item을 사용하겠다.
            //itemType을 인자로 전달 0~9까지 마녀 맵 스크롤 0~9사이 숫자로 스크롤의 등급을 매김 
            Net.SendUseItemPacket(1);
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            // 스크롤을 얻겠다. 얻는 스크롤은 서버에서 랜덤하게 부여
            Net.SendTeleportPacket(3);
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            PlayerPrefs.DeleteAll();
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
                            MN.ChangeMusicName(soundManager.getSongName());
                            playerAnim.SetFloat("Speed", PlayerPrefs.GetFloat("pAnimSpeed") + myPlayerID);
                            outlineAnim.SetFloat("Speed", PlayerPrefs.GetFloat("pAnimSpeed") + myPlayerID);
                            myPlayerID = p.id;
                            Objects[p.id] = player;

                            PlayerPrefs.SetString("myName" + myPlayerID, System.Text.Encoding.UTF8.GetString(p.name));
                            //print(p.cur_skill_type);
                            player.GetComponentInParent<FieldPlayerManager>().ChangeSkill(p.cur_skill_type, p.cur_skill_level);
                            player.GetComponentInParent<FieldPlayerManager>().SetSkillLevelContainer(p.skill_progress[0], p.skill_progress[1], p.skill_progress[2]);

                            for (int i = 0; i < 20; ++i)
                            {
                                Debug.Log(p.inventory[i]);
                            }
                            PlayerPrefs.SetInt("Money" + myPlayerID, p.money);

                            //PutPlayerObject(p.type, p.id, p.x, p.y);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_LOGIN_FAIL:
                        {
                            Protocol.sc_packet_login_fail p = Protocol.sc_packet_login_fail.SetByteToVar(data);
                            if (p.reason == 0)
                            {
                                Debug.LogError("해당 아이디는 이미 사용 중 입니다.");
                            }
                            else
                            {
                                Debug.LogError("하여튼 뭔가 잘못됨");
                            }
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
                            if (p.id < Protocol.CONSTANTS.MAX_USER)
                            {
                                if (p.id == myPlayerID)
                                {
                                    Objects[p.id].GetComponentInParent<FieldPlayerManager>().PlayerSpinDirection(p.x, p.y, p.z);
                                    Objects[p.id].GetComponentInParent<FieldPlayerManager>().JumpTrig();
                                    grid.cellMaps.Get(p.x, p.y, p.z).obejct.GetComponentInChildren<SpriteRenderer>().enabled = false;
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

                                        Objects[p.id].GetComponent<FieldHexCellPosition>().SetPosition(p.x, p.y, p.z);

                                        //print("myPlayerID : " + myPlayerID + " p.id :" + p.id + " p.skillType : " + p.skillType + " direction : " + p.direction);

                                        if (p.id == myPlayerID)
                                        {
                                            Objects[p.id].GetComponentInParent<FieldPlayerManager>().selfDirection = (HexDirection)p.direction;
                                            Objects[p.id].GetComponentInParent<FieldPlayerManager>().self_skillnum = p.skillType;
                                            Objects[p.id].GetComponentInParent<FieldPlayerManager>().self_skillLevel = p.skillLevel;
                                        }   
                                        else
                                        {
                                            Objects[p.id].GetComponentInChildren<Animator>().SetFloat("Speed", PlayerPrefs.GetFloat("pAnimSpeed"+myPlayerID));

                                            Objects[p.id].GetComponent<FieldOtherPlayerManager>().selfCoord.direction = (HexDirection)p.direction;
                                            Objects[p.id].GetComponent<FieldOtherPlayerManager>().other_playerName = System.Text.Encoding.UTF8.GetString(p.name);
                                            Objects[p.id].GetComponent<FieldOtherPlayerManager>().other_skillnum = p.skillType;
                                            Objects[p.id].GetComponent<FieldOtherPlayerManager>().other_skillLevel = p.skillLevel;
                                            //Debug.Log(p.id + " 플레이어 넣음");
                                            Objects[p.id].GetComponent<FieldOtherPlayerManager>().pID = p.id;
                                        }

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

                            if (p.id == myPlayerID)
                            {
                                Objects[p.id].GetComponentInParent<FieldPlayerManager>().ChangeSkill(p.skill_type, p.skill_level);
                            }
                            else
                            {
                                Objects[p.id].GetComponent<FieldOtherPlayerManager>().ChangeSkill(p.skill_type, p.skill_level);
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
                             * 4 상대가 파티가 이미 있는경우
                             * 5 내가 이미 파티가 있는대 파티 신청을 보낸경우
                             *  - CS_PACKET_PARTY_REQUEST_ANWSER패킷을 서버에 보낼 때  requester id가 -1이면 파티에 탈퇴하려고 하는 걸로 알고 서버에서 처리함
                             */
                            switch (p.anwser)
                            {
                                case 0:
                                    chattingManager.SetMess("<color=red>상대가 파티 신청을 거절했습니다!</color>");
                                    break;

                                case 1:
                                    chattingManager.SetMess("<color=red>" + p.p_id + "님이 새로운 파티원이 되었습니다!</color>");
                                    foreach (var i in p.ids)
                                    {
                                        if (i == myPlayerID || i == -1)
                                            continue;
                                        //print(i + " of skill : " + Objects[i].GetComponent<FieldOtherPlayerManager>().other_skillnum);
                                        PartyManager.instance.SetParty(i, Objects[i].GetComponent<FieldOtherPlayerManager>().other_skillnum);
                                    }
                                    break;

                                case 2:
                                    chattingManager.SetMess("<color=red>상대의 파티에 남은 자리가 없습니다!</color>");
                                    break;

                                case 3:
                                    chattingManager.SetMess("<color=red>" + p.p_id + "님이 파티에서 탈퇴하셨습니다.</color>");
                                    PartyManager.instance.DelParty(p.p_id);
                                    if (p.p_id == myPlayerID)
                                    {
                                        PartyManager.instance.DelParty();
                                    }
                                    break;

                                case 4:
                                    chattingManager.SetMess("<color=red>상대는 이미 파티가 있습니다!</color>");
                                    break;
                                case 5:
                                    chattingManager.SetMess("<color=red>이미 가입된 파티가 있습니다!</color>");

                                    break;
                            }
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_CHAT:
                        {
                            Protocol.sc_packet_chat p = Protocol.sc_packet_chat.SetByteToVar(data);

                            string mess = System.Text.Encoding.UTF8.GetString(p.mess).Split('\0')[0];
                            //Debug.Log(System.Text.Encoding.UTF8.GetString(p.mess));
                            chattingManager.SetMess(p.p_id + ": " + mess);
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_BUY_RESULT:
                        {
                            Protocol.sc_packet_buy_result p = Protocol.sc_packet_buy_result.SetByteToVar(data);

                            if (p.result == 0)
                            {
                                chattingManager.SetMess("소지금이 부족해 구매하지 못했습니다!");
                            }
                            else
                            {
                                switch (p.itemType)
                                {
                                    case 0:
                                        player.GetComponentInParent<FieldPlayerManager>().SetSkillLevelContainer(0, 0);
                                        PlayerPrefs.SetInt("Money" + myPlayerID, PlayerPrefs.GetInt("Money" + myPlayerID) - shopPrices.Skill1Prices[0]);
                                        break;
                                    case 1:
                                        player.GetComponentInParent<FieldPlayerManager>().SetSkillLevelContainer(0, 1);
                                        PlayerPrefs.SetInt("Money" + myPlayerID, PlayerPrefs.GetInt("Money" + myPlayerID) - shopPrices.Skill1Prices[1]);
                                        break;
                                    case 2:
                                        player.GetComponentInParent<FieldPlayerManager>().SetSkillLevelContainer(0, 2);
                                        PlayerPrefs.SetInt("Money" + myPlayerID, PlayerPrefs.GetInt("Money" + myPlayerID) - shopPrices.Skill1Prices[2]);
                                        break;
                                    case 3:
                                        player.GetComponentInParent<FieldPlayerManager>().SetSkillLevelContainer(0, 3);
                                        PlayerPrefs.SetInt("Money" + myPlayerID, PlayerPrefs.GetInt("Money" + myPlayerID) - shopPrices.Skill1Prices[3]);
                                        break;
                                    case 4:
                                        player.GetComponentInParent<FieldPlayerManager>().SetSkillLevelContainer(1, 0);
                                        PlayerPrefs.SetInt("Money" + myPlayerID, PlayerPrefs.GetInt("Money" + myPlayerID) - shopPrices.Skill2Prices[0]);
                                        break;
                                    case 5:
                                        player.GetComponentInParent<FieldPlayerManager>().SetSkillLevelContainer(1, 1);
                                        PlayerPrefs.SetInt("Money" + myPlayerID, PlayerPrefs.GetInt("Money" + myPlayerID) - shopPrices.Skill2Prices[1]);
                                        break;
                                    case 6:
                                        player.GetComponentInParent<FieldPlayerManager>().SetSkillLevelContainer(1, 2);
                                        PlayerPrefs.SetInt("Money" + myPlayerID, PlayerPrefs.GetInt("Money" + myPlayerID) - shopPrices.Skill2Prices[2]);
                                        break;
                                    case 7:
                                        player.GetComponentInParent<FieldPlayerManager>().SetSkillLevelContainer(1, 3);
                                        PlayerPrefs.SetInt("Money" + myPlayerID, PlayerPrefs.GetInt("Money" + myPlayerID) - shopPrices.Skill2Prices[3]);
                                        break;
                                    case 8:
                                        player.GetComponentInParent<FieldPlayerManager>().SetSkillLevelContainer(2, 0);
                                        PlayerPrefs.SetInt("Money" + myPlayerID, PlayerPrefs.GetInt("Money" + myPlayerID) - shopPrices.Skill3Prices[0]);
                                        break;
                                    case 9:
                                        player.GetComponentInParent<FieldPlayerManager>().SetSkillLevelContainer(2, 1);
                                        PlayerPrefs.SetInt("Money" + myPlayerID, PlayerPrefs.GetInt("Money" + myPlayerID) - shopPrices.Skill3Prices[1]);
                                        break;
                                    case 10:
                                        player.GetComponentInParent<FieldPlayerManager>().SetSkillLevelContainer(2, 2);
                                        PlayerPrefs.SetInt("Money" + myPlayerID, PlayerPrefs.GetInt("Money" + myPlayerID) - shopPrices.Skill3Prices[2]);
                                        break;
                                     case 11:
                                        player.GetComponentInParent<FieldPlayerManager>().SetSkillLevelContainer(2, 3);
                                        PlayerPrefs.SetInt("Money" + myPlayerID, PlayerPrefs.GetInt("Money" + myPlayerID) - shopPrices.Skill3Prices[3]);
                                        break;
                                }
                            }
                            // p->itemType  구매 시도한 아이템
                            // p->result    0 구매 실패 1 구매 성공
                        }
                        break;
                    case Protocol.CONSTANTS.SC_PACKET_USE_ITEM:
                        // user가 itemType을 사용했습니다.
                        // 필드에 있는 모든 플레이어에게 전송되는 패킷
                        {
                            Protocol.sc_packet_use_item p = Protocol.sc_packet_use_item.SetByteToVar(data);

                            Debug.Log(p.user + "가 " + p.itemType + "을 사용했습니다.");
                            Orgel.instance.Touch(p.itemType);
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
        yield return new WaitForSeconds(0.5f);
        player.GetComponentInParent<FieldPlayerManager>().PortalPlane.transform.SetParent(null);
        player.GetComponentInParent<FieldPlayerManager>().PortalPlane.transform.eulerAngles = new Vector3(0, 0, 0);
        //player.GetComponent<FieldPlayerManager>().PortalPlane.transform.localRotation = Quaternion.Euler(90,0,0);
        player.GetComponentInParent<FieldPlayerManager>().PortalPlane.SetActive(true);

        yield return new WaitForSeconds(1.5f);
        player.GetComponentInParent<FieldPlayerManager>().EnterPortal();
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(scene_num);
    }
}
