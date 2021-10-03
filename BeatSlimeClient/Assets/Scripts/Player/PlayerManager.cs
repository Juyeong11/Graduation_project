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

    private playerState state;
    public PlayerKeyHandler handle;     //Invalid

    public HexCellPosition selfCoord;
    public HexGrid grid;


    void Start()
    {
        state = playerState.Idle;
        onPlayerFly.Invoke();
        isFly = true;
    }

    void Update()
    {
        KeyHandler();

        //DEBUG
        gameObject.transform.position = new Vector3(gameObject.transform.position.x,gameObject.transform.position.y - 2f * Time.deltaTime, gameObject.transform.position.z);
        if (gameObject.transform.position.y < 0f)
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, 2f, gameObject.transform.position.z);
            if (isFly)
            {
                onPlayerStand.Invoke();
                isFly = false;
            }
        }
    }

    void KeyHandler()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z + 1).state != cellState.None)
                selfCoord.plus(-1, 0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y-1, selfCoord.coordinates.Z + 1).state != cellState.None)
                selfCoord.plus(0, -1, 1);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y - 1, selfCoord.coordinates.Z).state != cellState.None)
                selfCoord.plus(1, -1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (grid.cellMaps.Get(selfCoord.coordinates.X - 1, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z).state != cellState.None)
                selfCoord.plus(-1, 1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (grid.cellMaps.Get(selfCoord.coordinates.X, selfCoord.coordinates.Y + 1, selfCoord.coordinates.Z - 1).state != cellState.None)
                selfCoord.plus(0, 1, -1);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (grid.cellMaps.Get(selfCoord.coordinates.X + 1, selfCoord.coordinates.Y, selfCoord.coordinates.Z - 1).state != cellState.None)
                selfCoord.plus(1, 0, -1);
        }
    }

}
