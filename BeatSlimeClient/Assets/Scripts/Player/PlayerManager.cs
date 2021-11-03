using UnityEngine;
using UnityEngine.Events;

public enum playerState
{
    Idle,
    move,
}

public class PlayerManager : MonoBehaviour
{
    public UnityEvent onPlayerStand;
    public UnityEvent onPlayerFly;
    private bool isFly = false;

    public playerState state;
    public PlayerKeyHandler handle;     //Invalid

    public HexCellPosition selfCoord;
    public HexDirection selfDirection;
    public HexGrid grid;

    public void Start()
    {
        grid = GameManager.data.grid;
        state = playerState.Idle;
        selfDirection = HexDirection.Up;
        onPlayerFly.Invoke();
        isFly = true;
    }
    public void LoginOk()
    {
        grid = GameManager.data.grid;
        state = playerState.Idle;
        selfDirection = HexDirection.Up;
        onPlayerFly.Invoke();
        isFly = true;
    }

    void Update()
    {
        if (GameManager.data.isGameStart)
        {
            KeyHandler();
        }

    }

    void FixedUpdate()
    {
        //DEBUG
        if (GameManager.data.isGameStart)
            resetPosition();
        else
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 2f * Time.fixedDeltaTime, gameObject.transform.position.z);

        if (gameObject.transform.position.y < -0.1f)
        {
            if (isFly)
            {
                onPlayerStand.Invoke();
                isFly = false;
            }
        }

    }

    public void Beat()
    {
        grid.pPosition = selfCoord.coordinates;
    }

    bool KeyCheck()
    {
        if (GameManager.data.getIsGoodTiming() != 0)
        {
            return true;
        }
        return false;
    }
    public void PlayerMove(int x,int y, int z)
    {
        //GameManager.data.setMoved();
       // if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).state != cellState.None)
            selfCoord.SetPosition(x, y, z);
        //selfDirection = HexDirection.LeftUp;
    }
    void KeyHandler()
    {
        if (Input.GetKeyDown(KeyCode.Q) && KeyCheck())
        {
            if (GameManager.data.Net.isOnline)
            {
                // 서버에 이동 전송
                GameManager.data.Net.SendMovePacket((byte)Protocol.DIR.LEFTUP);
                selfDirection = HexDirection.LeftUp;

            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).state != cellState.None)
                    selfCoord.plus(-1, 0, 1);
                selfDirection = HexDirection.LeftUp;
            }


          
        }
        else if (Input.GetKeyDown(KeyCode.W) && KeyCheck())
        {
            if (GameManager.data.Net.isOnline)
            {
                // 서버에 이동 전송
                GameManager.data.Net.SendMovePacket((byte)Protocol.DIR.UP);
                selfDirection = HexDirection.Up;

            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z + 1).state != cellState.None)
                    selfCoord.plus(0, -1, 1);
                selfDirection = HexDirection.Up;
            }
        }
        else if (Input.GetKeyDown(KeyCode.E) && KeyCheck())
        {
            if (GameManager.data.Net.isOnline)
            {
                // 서버에 이동 전송
                GameManager.data.Net.SendMovePacket((byte)Protocol.DIR.RIGHTUP);
                selfDirection = HexDirection.RightUp;

            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z).state != cellState.None)
                    selfCoord.plus(1, -1, 0);
                selfDirection = HexDirection.RightUp;
            }
        }
        else if (Input.GetKeyDown(KeyCode.A) && KeyCheck())
        {
            if (GameManager.data.Net.isOnline)
            {
                // 서버에 이동 전송
                GameManager.data.Net.SendMovePacket((byte)Protocol.DIR.LEFTDOWN);
                selfDirection = HexDirection.LeftDown;

            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z).state != cellState.None)
                    selfCoord.plus(-1, 1, 0);
                selfDirection = HexDirection.LeftDown;
            }
        }
        else if (Input.GetKeyDown(KeyCode.S) && KeyCheck())
        {
            if (GameManager.data.Net.isOnline)
            {
                // 서버에 이동 전송
                GameManager.data.Net.SendMovePacket((byte)Protocol.DIR.DOWN);
                selfDirection = HexDirection.Down;

            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z - 1).state != cellState.None)
                    selfCoord.plus(0, 1, -1);
                selfDirection = HexDirection.Down;
            }
        }
        else if (Input.GetKeyDown(KeyCode.D) && KeyCheck())
        {
            if (GameManager.data.Net.isOnline)
            {
                // 서버에 이동 전송
                GameManager.data.Net.SendMovePacket((byte)Protocol.DIR.RIGHTDOWN);
                selfDirection = HexDirection.RightDown;

            }
            else
            {
                GameManager.data.setMoved();
                if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z - 1).state != cellState.None)
                    selfCoord.plus(1, 0, -1);
                selfDirection = HexDirection.RightDown;
            }
        }
    }


    void resetPosition()
    {
        //gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0f, gameObject.transform.position.z);

        gameObject.transform.position = new Vector3(gameObject.transform.position.x, (2f * GameManager.data.beatCounter / GameManager.data.timeByBeat) - 0.1f, gameObject.transform.position.z);
    }
}
