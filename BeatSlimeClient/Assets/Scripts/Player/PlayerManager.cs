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

    void Start()
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

        if (gameObject.transform.position.y < 0.5f)
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

    void KeyHandler()
    {
        if (Input.GetKeyDown(KeyCode.Q) && KeyCheck())
        {
            GameManager.data.setMoved();
            if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).state != cellState.None)
                selfCoord.plus(-1, 0, 1);
            selfDirection = HexDirection.LeftUp;
        }
        else if (Input.GetKeyDown(KeyCode.W) && KeyCheck())
        {
            GameManager.data.setMoved();
            if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z + 1).state != cellState.None)
                selfCoord.plus(0, -1, 1);
            selfDirection = HexDirection.Up;
        }
        else if (Input.GetKeyDown(KeyCode.E) && KeyCheck())
        {
            GameManager.data.setMoved();
            if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z).state != cellState.None)
                selfCoord.plus(1, -1, 0);
            selfDirection = HexDirection.RightUp;
        }
        else if (Input.GetKeyDown(KeyCode.A) && KeyCheck())
        {
            GameManager.data.setMoved();
            if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z).state != cellState.None)
                selfCoord.plus(-1, 1, 0);
            selfDirection = HexDirection.LeftDown;
        }
        else if (Input.GetKeyDown(KeyCode.S) && KeyCheck())
        {
            GameManager.data.setMoved();
            if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z - 1).state != cellState.None)
                selfCoord.plus(0, 1, -1);
            selfDirection = HexDirection.Down;
        }
        else if (Input.GetKeyDown(KeyCode.D) && KeyCheck())
        {
            GameManager.data.setMoved();
            if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z - 1).state != cellState.None)
                selfCoord.plus(1, 0, -1);
            selfDirection = HexDirection.RightDown;
        }
    }


    void resetPosition()
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, (2f * GameManager.data.beatCounter / GameManager.data.timeByBeat) + 0.5f, gameObject.transform.position.z);
    }
}
