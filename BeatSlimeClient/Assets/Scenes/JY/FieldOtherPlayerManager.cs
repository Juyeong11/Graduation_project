using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


public class FieldOtherPlayerManager : MonoBehaviour
{
    public Animator JumpTrigger;

    bool isReady = false;

    public playerState state;
    public PlayerKeyHandler handle;     //Invalid

    public FieldHexCellPosition selfCoord;
 
    public FieldHexGrid grid;

    public Transform PlayerTransform;

    public int pID;
    public string other_playerName;
    public int other_skillnum;
    public int other_skillLevel;

    public void Start()
    {
        //pID = 0;
        grid = FieldGameManager.data.grid;
        state = playerState.Idle;
        //selfCoord.direction = HexDirection.Up;
        //other_skillnum = 1;
        //onPlayerFly.Invoke();
    }
    public void LoginOk()
    {
        grid = FieldGameManager.data.grid;
        state = playerState.Idle;
        //selfCoord.direction = HexDirection.Up;
        //onPlayerFly.Invoke();
    }

    public void JumpTrig()
    {
        JumpTrigger.SetTrigger("Jump");
    }

    void Update()
    {
        //DEBUG
        //if (GameManager.data.isGameStart)
        //    resetPosition();
        //else
        //   gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y -2.f*Time.deltaTime, gameObject.transform.position.z);

        /*        if (gameObject.transform.position.y < -0.1f)
                {
                    if (isFly)
                    {
                        onPlayerStand.Invoke();
                        isFly = false;
                    }
                }*/

        //if (GameManager.data.Net.isOnline)
        //{
        //    GameManager.data.PlaySound();
        //}

        if (FieldGameManager.data.isGameStart)
        {
            PlayerRotateToLookAt();
            PlayerWCheck();
            //PlayerPortalCheck();
        }
    }
    public void PlayerPortalCheck()
    {
        // 클라에서 플레이어가 포탈에 있는지 위치 검사 후 맞다면 -> 서버에 패킷 전송 -> 서버에서 해당 좌표가 포탈이 맞는지 확인
        // -> 맞다면 ready상태 이펙트 패킷 전송 -> 3명이 다 준비되면 씬 전환

        // 포탈 타일인지 확인 지금은 0,0,0     1,0,-1     0,1,-1로 함 -> 씬 전환 잘되면 Cell에 type 추가해 비교하는 방법으로 바꾸자
        if(selfCoord.coordinates.X == 3 && selfCoord.coordinates.Z == -3 ||
            selfCoord.coordinates.X == 3 && selfCoord.coordinates.Z == -2 ||
            selfCoord.coordinates.X == 2 && selfCoord.coordinates.Z == -2)
        {
            if (isReady) return;
            FieldGameManager.Net.SendChangeSceneReadyPacket(1);
            isReady = true;
            return;
        }
        else if (isReady)
        {
            FieldGameManager.Net.SendChangeSceneReadyPacket(0);
        }
        isReady = false;
    }

    public void PlayerWCheck()
    {
        if (selfCoord.coordinates.W != grid.cellMaps.Get(selfCoord.coordinates).w + 1)
        {
            //Debug.LogError(">Player W Coordinate Error!<");
            print("Self W : " + selfCoord.coordinates.W);
            print("Self W : " + grid.cellMaps.Get(selfCoord.coordinates).w);
            selfCoord.coordinates.W = grid.cellMaps.Get(selfCoord.coordinates).w + 1;
        }
        PlayerTransform.position = calculatePlayerPosition();

        //Debug.Log("z : " + gameObject.transform.position.z);

    }

    public Vector3 calculatePlayerPosition()
    {
        int beatTime = 500;

        float tick = LerpSquare((Time.time - selfCoord.preBeatedTime) * 1000f / beatTime);
        float newX;
        float newY;
        float newZ;

        newX = Mathf.Lerp(selfCoord.preCoordinates.X * 0.866f, selfCoord.coordinates.X * 0.866f, tick);
        newZ = Mathf.Lerp(selfCoord.preCoordinates.X * 0.5f + selfCoord.preCoordinates.Z, selfCoord.coordinates.X * 0.5f + selfCoord.coordinates.Z, tick);
        newY = SlimeWLerp(HexCellPosition.calculateWPosition(selfCoord.preCoordinates.W), HexCellPosition.calculateWPosition(selfCoord.coordinates.W), tick);

        if (tick >= 1f)
        {
            selfCoord.preCoordinates = selfCoord.coordinates;
        }

        return new Vector3(newX, newY, newZ);
    }

    public float LerpSquare(float tick)
    {
        if (tick < 0.3f)
            return 0f;
        else if (tick < 0.7f)
        {
            return (tick - 0.3f) * 2.5f;
        }
        else
            return 1f;
    }

    public float SlimeWLerp(float a, float b, float t)
    {
        float skyHigh = (a + b) * 0.5f + 2f;

        if (t < 0.5f)
        {
            return Mathf.Lerp(a, skyHigh, t);
        }
        else
        {
            return Mathf.Lerp(skyHigh, b, t);
        }
    }


    public void PlayerRotateToLookAt()
    {
        switch(selfCoord.direction)
        {
            case HexDirection.LeftUp:
                //시야 벡터를 외적해야지 각도를 외적하면 당연히 틀리지
                //Vector3 c = Vector3.Cross(transform.rotation.eulerAngles, new Vector3(0, -120, 0));
                //transform.Rotate(0, c.x * 3f, 0);

                transform.rotation = Quaternion.Euler(new Vector3(0, -120 - 90, 0));
                break;
            case HexDirection.Up:
                transform.rotation = Quaternion.Euler(new Vector3(0, -90 - 90, 0));
                break;
            case HexDirection.RightUp:
                transform.rotation = Quaternion.Euler(new Vector3(0, -30 - 90, 0));
                break;
            case HexDirection.Down:
                transform.rotation = Quaternion.Euler(new Vector3(0, 90 - 90, 0));
                break;
            case HexDirection.LeftDown:
                transform.rotation = Quaternion.Euler(new Vector3(0, 120 - 90, 0));
                break;
            case HexDirection.RightDown:
                transform.rotation = Quaternion.Euler(new Vector3(0, 30 - 90, 0));
                break;
        }
    }

    public void PlayerSpinDirection(int x, int y, int z)
    {
        if (selfCoord.coordinates.X - 1 == x && selfCoord.coordinates.Z + 1 == z)
        {
            selfCoord.direction = HexDirection.LeftUp;
        }
        else if (selfCoord.coordinates.X == x && selfCoord.coordinates.Z + 1 == z)
        {
            selfCoord.direction = HexDirection.Up;
        }
        else if (selfCoord.coordinates.X + 1 == x && selfCoord.coordinates.Z == z)
        {
            selfCoord.direction = HexDirection.RightUp;
        }
        else if (selfCoord.coordinates.X - 1 == x && selfCoord.coordinates.Z == z)
        {
            selfCoord.direction = HexDirection.LeftDown;
        }
        else if (selfCoord.coordinates.X == x && selfCoord.coordinates.Z - 1 == z)
        {
            selfCoord.direction = HexDirection.Down;
        }
        else if (selfCoord.coordinates.X + 1 == x && selfCoord.coordinates.Z - 1 == z)
        {
            selfCoord.direction = HexDirection.RightDown;
        }
    }

    public void Beat()
    {
        //Debug.Log("BEAT");
        grid.pPosition = selfCoord.coordinates;
        selfCoord.beat();
    }

    bool KeyCheck(KeyCode k)
    {
        switch (k)
        {
            case KeyCode.W:
                selfCoord.direction = HexDirection.LeftUp;
                break;
            case KeyCode.E:
                selfCoord.direction = HexDirection.Up;
                break;
            case KeyCode.R:
                selfCoord.direction = HexDirection.RightUp;
                break;
            case KeyCode.S:
                selfCoord.direction = HexDirection.LeftDown;
                break;
            case KeyCode.D:
                selfCoord.direction = HexDirection.Down;
                break;
            case KeyCode.F:
                selfCoord.direction = HexDirection.RightDown;
                break;
        }

            return true;
    }
    public void PlayerMove(int x, int y, int z)
    {
        //GameManager.data.setMoved();
        // if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).state != cellState.None)
        selfCoord.SetPosition(x, y, z);
        //selfDirection = HexDirection.LeftUp;
    }
    
    public void ChangeSkill(int skillNum)
    {
        other_skillnum = skillNum;
    }
}
