using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public enum enemyState
{
    Idle,
    move,
    guard,
}

public class EnemyManager : MonoBehaviour
{
    private enemyState state;
    public int enemyHp;
    public int enemyMaxHp;

    public HexCellPosition selfCoord;
    public HexGrid grid;

    //얘네 int가 아니라 다른거여야 할 필요가 있을지도
    public Dictionary<Beat, int> enemyMovingList;
    public Dictionary<Beat, int> enemyAttackList;
    public Dictionary<Beat, int> enemyNoteList;

    void Start()
    {
        grid = GameManager.data.grid;
        state = enemyState.Idle;
    }

    void Update()
    {
        
    }
}
