using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public Image EnemyHPImage;
    public Image EnemyPrevHPImage;
    public HPManager HP;


    void Start()
    {
        HP = new HPManager();
        grid = GameManager.data.grid;
        state = enemyState.Idle;

        HP.Initialized(true);
        //selfCoord.SetPosition()
    }

    void Update()
    {
        reflectPosition();
        EnemyWCheck();
        CalcHPSlider();
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
    public void CalcHPSlider()
    {
        HP.hpUpdate();
        EnemyHPImage.fillAmount = (float)HP.CurrentHP / (float)HP.MaxHp;
        EnemyPrevHPImage.fillAmount = (float)HP.prevHP / (float)HP.MaxHp;
    }
}
