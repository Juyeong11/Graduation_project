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
    public GameObject nearestSlimeCoord;
    public bool doseNotConsiderSlime;
    private bool ticker = false;
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

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "InGameScene03")
            EnemyRotateToLookAt();
    }

    public void EnemyRotateToLookAt()
    {
        if (nearestSlimeCoord == null || doseNotConsiderSlime)
        {
            switch (selfCoord.direction)
            {
                case HexDirection.LeftUp:
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
        else
        {
            //if (spinningLerpFactor < 1f)
            Vector3 selfVector = -transform.forward;
            selfVector.y = 0;
            Vector3 targetVector = nearestSlimeCoord.transform.position - transform.position;
            targetVector.y = 0;
            float cross = Vector3.Cross(selfVector, targetVector).y;
            if (cross > 0.2f || cross < 0.2f)
            {
                //print("rotate : " + Time.deltaTime * 100f);
                transform.Rotate(0, Time.deltaTime * 100f * cross, 0);
                ticker = false;
            }
            else
            {
                if (!ticker)
                {
                    Vector3 eyesPacker = new Vector3 (nearestSlimeCoord.transform.position.x, transform.position.y, nearestSlimeCoord.transform.position.z);
                    transform.LookAt(eyesPacker);
                    ticker = true;
                }
            }
        }
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

    public void BeatPatternServe(Beat NowBeat, Beat offset, GameObject destination)
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
        HP.hpUpdate(Time.deltaTime);
        EnemyHPImage.fillAmount = (float)HP.CurrentHP / (float)HP.MaxHp;
        EnemyPrevHPImage.fillAmount = (float)HP.prevHP / (float)HP.MaxHp;
    }

    public void SetNearestSlime(GameObject slime)
    {
        nearestSlimeCoord = slime;
    }
}
