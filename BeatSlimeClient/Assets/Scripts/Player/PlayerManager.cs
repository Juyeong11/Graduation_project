using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;

public enum playerState
{
    Idle,
    move,
}

public class PlayerManager : MonoBehaviour
{
    public bool isDebug;

    public Animator JumpTrigger;
    public UnityEvent onPlayerStand;
    public UnityEvent onPlayerFly;

    public playerState state;
    public PlayerKeyHandler handle;     //Invalid

    public HexCellPosition selfCoord;
    public HexDirection selfDirection;
    public HexGrid grid;

    public Transform PlayerTransform;

    Image PlayerHPImage;
    Image PlayerPrevHPImage;

    public HPManager HP;

    bool playerAttacking;
    List<(Beat,float)> SettledBallBeats;

    public bool isThisCurrentPlayingPlayerObject;

    public void Start()
    {
        HP = new HPManager();
        playerAttacking = false;
        grid = GameManager.data.grid;
        state = playerState.Idle;
        selfDirection = HexDirection.Up;
        SettledBallBeats = new List<(Beat,float)>();

        HP.Initialized(false);
        //onPlayerFly.Invoke();
    }
    public void LoginOk()
    {
        grid = GameManager.data.grid;
        state = playerState.Idle;
        selfDirection = HexDirection.Up;
        //onPlayerFly.Invoke();
    }

    public void SetHPImages(Image hp, Image prev)
    {
        PlayerHPImage = hp;
        PlayerPrevHPImage = prev;
    }

    public void JumpTrig()
    {
        JumpTrigger.SetTrigger("Jump");
    }

    void Update()
    {
        if (GameManager.data.isGameStart)
        {
            playerAttacking = false;

            if (isThisCurrentPlayingPlayerObject)
            {
                KeyHandler();
                BallBeatCheck();

            }
            PlayerRotateToLookAt();
            PlayerWCheck();
            CalcHPSlider();
        }
    }

    public void PlayerWCheck()
    {
        if (selfCoord.coordinates.W != grid.cellMaps.Get(selfCoord.coordinates).w + 1)
        {
            //if (isThisCurrentPlayingPlayerObject)
            //{
            //    Debug.LogError(">Player W Coordinate Error!< [ Self W : " + selfCoord.coordinates.W + ", Cell W : " + grid.cellMaps.Get(selfCoord.coordinates).w + 1 + " ]");
            //}
            //print("Self W : " + selfCoord.coordinates.W);
            selfCoord.coordinates.W = grid.cellMaps.Get(selfCoord.coordinates).w + 1;

        }
        PlayerTransform.position = calculatePlayerPosition();

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
            case KeyCode.W:
                selfDirection = HexDirection.LeftUp;
                break;
            case KeyCode.E:
                selfDirection = HexDirection.Up;
                break;
            case KeyCode.R:
                selfDirection = HexDirection.RightUp;
                break;
            case KeyCode.S:
                selfDirection = HexDirection.LeftDown;
                break;
            case KeyCode.D:
                selfDirection = HexDirection.Down;
                break;
            case KeyCode.F:
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

        if (isDebug)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                HP.CurrentHP -= 45;
            }
        }

        if (Input.GetKeyDown(KeyCode.W) && KeyCheck(KeyCode.W))
        {
            if (FieldGameManager.Net.isOnline)
            {
                //Debug.Log("키 전송");
                // 서버에 이동 전송
                GameManager.data.setMoved();

                FieldGameManager.Net.SendMovePacket((byte)Protocol.DIR.LEFTUP);
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
        else if (Input.GetKeyDown(KeyCode.E) && KeyCheck(KeyCode.E))
        {
            if (FieldGameManager.Net.isOnline)
            {
                GameManager.data.setMoved();
                // 서버에 이동 전송
                FieldGameManager.Net.SendMovePacket((byte)Protocol.DIR.UP);
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
        else if (Input.GetKeyDown(KeyCode.R) && KeyCheck(KeyCode.R))
        {
            if (FieldGameManager.Net.isOnline)
            {
                GameManager.data.setMoved();
                // 서버에 이동 전송
                FieldGameManager.Net.SendMovePacket((byte)Protocol.DIR.RIGHTUP);
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
        else if (Input.GetKeyDown(KeyCode.S) && KeyCheck(KeyCode.S))
        {
            if (FieldGameManager.Net.isOnline)
            {
                GameManager.data.setMoved();
                // 서버에 이동 전송
                FieldGameManager.Net.SendMovePacket((byte)Protocol.DIR.LEFTDOWN);
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
        else if (Input.GetKeyDown(KeyCode.D) && KeyCheck(KeyCode.D))
        {
            if (FieldGameManager.Net.isOnline)
            {
                GameManager.data.setMoved();
                // 서버에 이동 전송
                FieldGameManager.Net.SendMovePacket((byte)Protocol.DIR.DOWN);
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
        else if (Input.GetKeyDown(KeyCode.F) && KeyCheck(KeyCode.F))
        {
            if (FieldGameManager.Net.isOnline)
            {
                GameManager.data.setMoved();
                // 서버에 이동 전송
                FieldGameManager.Net.SendMovePacket((byte)Protocol.DIR.RIGHTDOWN);
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
        else if (Input.GetKeyDown(KeyCode.J) && KeyCheck(KeyCode.J))
        {
            if (FieldGameManager.Net.isOnline)
            {
                GameManager.data.setMoved();
                // 서버에 공격 전송
                FieldGameManager.Net.SendMovePacket((byte)Protocol.DIR.RIGHTDOWN);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (FieldGameManager.Net.isOnline)
            {
                // 서버에 패링 전송
                FieldGameManager.Net.SendParryingPacket();
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
                GameManager.data.MidANote.noteEnd();
            }
            else if (SettledBallBeats[0].Item1.GetBeatTime() - GameManager.data.JudgementTiming < GameManager.data.nowBeat.GetBeatTime()
                && playerAttacking)
            {
                //공격
                Debug.Log("반격 성공!");
                VFXManager.data.HitSounder(1);
                SettledBallBeats.RemoveAt(0);
                GameManager.data.MidANote.notePerfect();
            }
        }
    }


    //여기서 Lerp
    public Vector3 calculatePlayerPosition()
    {
        int beatTime = GameManager.data.timeByBeat;

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

    public void CalcHPSlider()
    {
        HP.hpUpdate(Time.deltaTime);
        PlayerHPImage.fillAmount = (float)HP.CurrentHP / (float)HP.MaxHp;
        PlayerPrevHPImage.fillAmount = (float)HP.prevHP / (float)HP.MaxHp;
    }
}
