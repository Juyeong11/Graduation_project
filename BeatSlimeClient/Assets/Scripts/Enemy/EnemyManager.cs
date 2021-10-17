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
    public enemyState state;
    public int enemyHp;
    public int enemyMaxHp;

    public HexCellPosition selfCoord;
    public HexGrid grid;

    public List<EMoving> enemyMovingList;
    public List<EAttack> enemyAttackList;
    public List<ENote> enemyNoteList;


    void Start()
    {
        grid = GameManager.data.grid;
        state = enemyState.Idle;
    }

    void Update()
    {
        
    }

    public void PatternServe()
    {
        //패턴 번호따라 애니메이션 등
        grid.EnemyAttack(PatternManager.data.CastedPattern);
    }
}
