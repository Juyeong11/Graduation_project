using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FieldInputManager : MonoBehaviour
{
    //-----------------------------------
    public List<GameObject> Menus;

    private int menuDuplicate;
    private int preHittedObject;
    private int HittedObject;

    private Vector3 pop = new Vector3(50, 50);
    private Vector3 bas = new Vector3(-150,0);

    //-----------------------------------

    public Animator PM;
    public CinemachineVirtualCamera CCO;
    CinemachineTransposer CT;
    Vector3 target;

    void Awake()
    {
        CT = CCO.GetCinemachineComponent<CinemachineTransposer>();
        preHittedObject = 0;
        menuDuplicate = 0;
        target = CT.m_FollowOffset;
    }
 
    void Update ()
    {
        //메뉴
        if(Input.GetMouseButtonDown(1))  // 마우스가 클릭 되면
        {
            //애니메이션으로 바꿀거
            Menus[menuDuplicate].GetComponent<BillboardUI>().GetOff();
            menuDuplicate ^= 1;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                HittedObject = hit.transform.GetHashCode();
                //print("something!");
                if (hit.collider.tag == "Slimes" && (preHittedObject != HittedObject || (Menus[0].activeSelf == false && Menus[1].activeSelf == false)))
                {
                    Menus[menuDuplicate].transform.position = hit.transform.position;
                    if (hit.transform.parent.gameObject != FieldGameManager.data.player)
                        Menus[menuDuplicate].GetComponent<BillboardUI>().GetOn(hit.transform, hit.transform.parent.GetComponent<FieldOtherPlayerManager>().pID);
                    else
                        Menus[menuDuplicate].GetComponent<BillboardUI>().GetOn(hit.transform, -1);
                    //print("hit!");
                }
                else if (hit.collider.tag == "Cells")
                {
                    //A* 이동
                    hit.transform.GetComponentInChildren<SpriteRenderer>().enabled = true;
                    HexCoordinates Destination = hit.transform.GetComponent<HexCellPosition>().coordinates;

                    FieldGameManager.Net.SendSetPath((short)Destination.X, (short)Destination.Z);
                    //FieldGameManager.data.player.GetComponent<FieldPlayerManager>().Destination = hit.transform.GetComponent<HexCellPosition>().coordinates;
                    //FieldGameManager.data.player.GetComponent<FieldPlayerManager>().ASTAR();

                }

                preHittedObject = HittedObject;
            }
        }

        CT.m_FollowOffset = Vector3.Lerp(CT.m_FollowOffset, target, Time.deltaTime * 2f);

        //카메라
        if (Input.GetKey(KeyCode.LeftBracket))
        {
            if (CT.m_FollowOffset.y < 10f)
            {
                target = new Vector3(CT.m_FollowOffset.x, CT.m_FollowOffset.y + 0.6f, CT.m_FollowOffset.z - 0.4f);
                // CT.m_FollowOffset.y += 0.3f;
                // CT.m_FollowOffset.z -= 0.2f;
            }
            
        }
        if (Input.GetKey(KeyCode.RightBracket))
        {
            if (CT.m_FollowOffset.y > 0.5f)
            {
                target = new Vector3(CT.m_FollowOffset.x, CT.m_FollowOffset.y - 0.6f, CT.m_FollowOffset.z + 0.4f);
                // CT.m_FollowOffset.y -= 0.3f;
                // CT.m_FollowOffset.z += 0.2f;
            }
        }

        if (Input.GetKey(KeyCode.Tab))
        {
            PM.SetBool("pop",true);
        }
        else
        {
            PM.SetBool("pop", false);
        }
    }
}
