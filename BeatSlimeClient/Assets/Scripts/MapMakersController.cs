using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class MapMakersController : MonoBehaviour
{
    public HexCellPosition playerPosition;
    public CinemachineVirtualCamera CCO;

    private void Start()
    {
        var CT = CCO.GetCinemachineComponent<CinemachineTransposer>();
        CT.m_FollowOffset.y = 3f;
        CT.m_FollowOffset.z = -4f;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            playerPosition.plus(0, 0, 0,1);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            playerPosition.plus(0, 0, 0,-1);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            playerPosition.plus(-1, 0, 1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            playerPosition.plus(0, -1, 1);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            playerPosition.plus(1, -1, 0);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            playerPosition.plus(-1, 1, 0);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            playerPosition.plus(0, 1, -1);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            playerPosition.plus(1, 0, -1);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            var CT = CCO.GetCinemachineComponent<CinemachineTransposer>();
            CT.m_FollowOffset.y += 0.3f;
            CT.m_FollowOffset.z -= 0.3f;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            var CT = CCO.GetCinemachineComponent<CinemachineTransposer>();
            CT.m_FollowOffset.y -= 0.3f;
            CT.m_FollowOffset.z += 0.3f;
        }


        gameObject.transform.position = playerPosition.getRealPosition();
    }
}
