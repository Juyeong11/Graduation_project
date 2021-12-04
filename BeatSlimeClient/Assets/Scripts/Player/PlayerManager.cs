using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public enum playerState
{
    Idle,
    move,
}

public class PlayerManager : MonoBehaviour
{
    public Animator JumpTrigger;
    public UnityEvent onPlayerStand;
    public UnityEvent onPlayerFly;

    private bool isFly = false;

    public playerState state;
    public PlayerKeyHandler handle;     //Invalid

    public HexCellPosition selfCoord;
    public HexDirection selfDirection;
    public HexGrid grid;

    public Transform PlayerTransform;

    bool playerAttacking;
    List<(Beat,float)> SettledBallBeats;

    public void Start()
    {
        playerAttacking = false;
        grid = GameManager.data.grid;
        state = playerState.Idle;
        selfDirection = HexDirection.Up;
        SettledBallBeats = new List<(Beat,float)>();
        //onPlayerFly.Invoke();
        isFly = true;
    }
    public void LoginOk()
    {
        grid = GameManager.data.grid;
        state = playerState.Idle;
        selfDirection = HexDirection.Up;
        //onPlayerFly.Invoke();
        isFly = true;
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

        if (GameManager.data.isGameStart)
        {
            playerAttacking = false;

            KeyHandler();
            PlayerRotateToLookAt();
            PlayerWCheck();
            BallBeatCheck();
        }
    }

    public void PlayerWCheck()
    {
        if (selfCoord.coordinates.W != grid.cellMaps.Get(selfCoord.coordinates).w + 1)
        {
            Debug.LogError(">Player W Coordinate Error!<");
            print("Self W : " + selfCoord.coordinates.W);
            selfCoord.coordinates.W = grid.cellMaps.Get(selfCoord.coordinates).w + 1;
        }
        PlayerTransform.position = selfCoord.calculatePlayerPosition();

        //Debug.Log("z : " + gameObject.transform.position.z);

    }

    public void PlayerRotateToLookAt()
    {
        switch(selfDirection)
        {
            case HexDirection.LeftUp:
                //시야 벡터를 외적해야지 각도를 외적하면 당연히 틀리지
                //Vector3 c = Vector3.Cross(transform.rotation.eulerAngles, new Vector3(0, -120, 0));
                //transform.Rotate(0, c.x * 3f, 0);

                transform.rotation = Quaternion.Euler(new Vector3(0, -120, 0));
                break;
            case HexDirection.Up:
                transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                break;
            case HexDirection.RightUp:
                transform.rotation = Quaternion.Euler(new Vector3(0, -30, 0));
                break;
            case HexDirection.Down:
                transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                break;
            case HexDirection.LeftDown:
                transform.rotation = Quaternion.Euler(new Vector3(0, 120, 0));
                break;
            case HexDirection.RightDown:
                transform.rotation = Quaternion.Euler(new Vector3(0, 30, 0));
                break;
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
            case KeyCode.Q:
                selfDirection = HexDirection.LeftUp;
                break;
            case KeyCode.W:
                selfDirection = HexDirection.Up;
                break;
            case KeyCode.E:
                selfDirection = HexDirection.RightUp;
                break;
            case KeyCode.A:
                selfDirection = HexDirection.LeftDown;
                break;
            case KeyCode.S:
                selfDirection = HexDirection.Down;
                break;
            case KeyCode.D:
                selfDirection = HexDirection.RightDown;
                break;
        }

        if (GameManager.data.getIsGoodTiming() != 0)
        {
            return true;
        }
        return false;
    }
    public void PlayerMove(int x, int y, int z)
    {
        //GameManager.data.setMoved();
        // if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).state != cellState.None)
        selfCoord.SetPosition(x, y, z);
        //selfDirection = HexDirection.LeftUp;
    }
    void KeyHandler()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerAttacking = true;
        }

        if (Input.GetKeyDown(KeyCode.Q) && KeyCheck(KeyCode.Q))
        {
            if (GameManager.data.Net.isOnline)
            {
                //Debug.Log("키 전송");
                // 서버에 이동 전송
                GameManager.data.setMoved();

                GameManager.data.Net.SendMovePacket((byte)Protocol.DIR.LEFTUP);
            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(-1, 0, 1, grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.W) && KeyCheck(KeyCode.W))
        {
            if (GameManager.data.Net.isOnline)
            {
                GameManager.data.setMoved();
                // 서버에 이동 전송
                GameManager.data.Net.SendMovePacket((byte)Protocol.DIR.UP);
            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z + 1).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z + 1).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(0, -1, 1, grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y -1, selfCoord.coordinates.Z + 1).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.E) && KeyCheck(KeyCode.E))
        {
            if (GameManager.data.Net.isOnline)
            {
                GameManager.data.setMoved();
                // 서버에 이동 전송
                GameManager.data.Net.SendMovePacket((byte)Protocol.DIR.RIGHTUP);
            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(1, -1, 0, grid.cellMaps.Get(selfCoord.coordinates.X+1, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                    
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.A) && KeyCheck(KeyCode.A))
        {
            if (GameManager.data.Net.isOnline)
            {
                GameManager.data.setMoved();
                // 서버에 이동 전송
                GameManager.data.Net.SendMovePacket((byte)Protocol.DIR.LEFTDOWN);
            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(-1, 1, 0, grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.S) && KeyCheck(KeyCode.S))
        {
            if (GameManager.data.Net.isOnline)
            {
                GameManager.data.setMoved();
                // 서버에 이동 전송
                GameManager.data.Net.SendMovePacket((byte)Protocol.DIR.DOWN);
            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z - 1).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z -1).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(0, 1, -1, grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z-1).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.D) && KeyCheck(KeyCode.D))
        {
            if (GameManager.data.Net.isOnline)
            {
                GameManager.data.setMoved();
                // 서버에 이동 전송
                GameManager.data.Net.SendMovePacket((byte)Protocol.DIR.RIGHTDOWN);
            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z - 1).state != cellState.None)
                {
                    if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z - 1).w <= selfCoord.coordinates.W)
                    {
                        selfCoord.plus(1, 0, -1, grid.cellMaps.Get(selfCoord.coordinates.X+1, selfCoord.coordinates.Y, selfCoord.coordinates.Z - 1).w - selfCoord.coordinates.W + 1);
                        JumpTrig();
                    }
                }
            }
        }

    }

    void resetPosition()
    {
        //gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0f, gameObject.transform.position.z);

        gameObject.transform.position = new Vector3(gameObject.transform.position.x, (2f * GameManager.data.beatCounter / GameManager.data.timeByBeat) - 0.1f, gameObject.transform.position.z);
    }

    public void SetBallBeat(Beat now,Beat b)
    {
        Debug.Log(GameManager.data.timeByBeat / (float)b.GetBeatTime() * 0.75f);
        SettledBallBeats.Add((now + b, GameManager.data.timeByBeat / (float)b.GetBeatTime() * 0.75f));
    }
    public void BallBeatCheck()
    {
        if (SettledBallBeats.Count > 0)
        {
            if (SettledBallBeats[0].Item1.GetBeatTime() + GameManager.data.JudgementTiming < GameManager.data.nowBeat.GetBeatTime())
            {
                //놓침
                Debug.Log("반격 실패");
                VFXManager.data.HitSounder(SettledBallBeats[0].Item2);
                SettledBallBeats.RemoveAt(0);
            }
            else if (SettledBallBeats[0].Item1.GetBeatTime() - GameManager.data.JudgementTiming < GameManager.data.nowBeat.GetBeatTime()
                && playerAttacking)
            {
                //공격
                Debug.Log("반격 성공!");
                VFXManager.data.HitSounder(1);
                SettledBallBeats.RemoveAt(0);
            }
        }
    }
}
