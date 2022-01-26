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
    public Animator selfAnim;

    public enemyState state;
    public int enemyHp;
    public int enemyMaxHp;

    public HexCellPosition selfCoord;
    public HexGrid grid;

    //Invalid
    public List<EMoving> enemyMovingList;
    public List<EAttack> enemyAttackList;

    //
    public List<BeatBall> enemyNoteList;


    void Start()
    {
        grid = GameManager.data.grid;
        state = enemyState.Idle;
    }

    void Update()
    {
        reflectPosition();
        EnemyWCheck();
    }

    public void EnemyWCheck()
    {
        //Debug.LogError(">Player W Coordinate Error!< [ Self W : " + selfCoord.coordinates.W + ", Cell W : " + grid.cellMaps.Get(selfCoord.coordinates).w + 1 + " ]");
        //print("Self W : " + selfCoord.coordinates.W);
        selfCoord.coordinates.W = grid.cellMaps.Get(selfCoord.coordinates).w + 1;
        //PlayerTransform.position = calculatePlayerPosition();

        //Debug.Log("z : " + gameObject.transform.position.z);

    }

    public void Beat()
    {
        grid.ePosition = selfCoord.coordinates;
    }

    public void PatternSet()
    {
        //여기서 애니메이션 처리
        grid.WarningCell(PatternManager.data.SettedPattern);

        //노트 타입따라 다른 애니메이션 만들 때 패턴 설정도 해 줘야 함 (id 사용)
    }

    public void PatternServe()
    {
        //여기서 장판데미지 처리
        grid.EnemyAttack(PatternManager.data.CastedPattern);
    }

    public void BeatPatternServe(Beat NowBeat,Beat offset,GameObject destination)
    {
        selfAnim.SetTrigger("Attack");

        //오브젝트 풀 만들어서 풀링하기
        enemyNoteList[0].Init(NowBeat, offset, gameObject, destination);
    }

    public void reflectPosition()
    {
        selfCoord.reflectPosition();
    }
}
