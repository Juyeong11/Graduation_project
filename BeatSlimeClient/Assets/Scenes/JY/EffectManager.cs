using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EffectManager : MonoBehaviour
{
    static public EffectManager instance;
    public GameObject TileEffectPrefab;

    public GameObject BossQuakeEffectPrefab;
    public GameObject BossWaterGunEffectPrefab;
    public GameObject BossTargetingEffectPrefab;
    public GameObject BossRokets;

    public GameObject PlayerQuakeEffectPrefab;
    public GameObject PlayerWaterGunEffectPrefab;
    public GameObject PlayerHealEffectPrefab;


    int[,] around = new int[6, 3] {
        {0, -1, 1 },{ 1, -1, 0 }, { 1, 0, -1 },
    { 0, 1, -1 },{ -1, 1, 0 }, { -1, 0, 1 } };

    Effects HexPattern;

    // Start is called before the first frame update
    void Start()
    {
        HexPattern = new Effects();
        instance = this;

    }
    private void OnDisable()
    {
        Destroy(BossRokets);
    }
    // 1방향 공격
    public void OneTileEffect(int startX, int startY, int startZ, int speed)
    {

        GameObject go = Instantiate(TileEffectPrefab, GameManager.data.grid.cellMaps.Get(startX, startY, startZ).getCellRealPosition(), Quaternion.identity);
        float s = speed * 1 / 1000f;
        //Debug.Log("effect t" + Time.time);
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "InGameScene03")
        { 
            go.GetComponent<TileEffect2>().initialize(s);
        }
        else
            go.GetComponent<TileEffect>().speed = s;

        //go.transform.SetParent(GameManager.data.grid.cellMaps.map_Get(startX, startZ).obejct.transform);

    }

    public void BossTileEffect(int startX, int startY, int startZ, int speed, int EffectType)
    {
        foreach (var pat in HexPattern.GetPattern(EffectType))
        {
            OneTileEffect(pat.x + startX, pat.y + startY, pat.z + startZ, speed);
        }
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "InGameScene03")
        {

            Animator Ani = GameManager.data.GetEnemyAnim();
            if (Ani.GetCurrentAnimatorStateInfo(0).IsName("RocketLaunch")) return;
            Ani.SetTrigger("StartLaunch");
            float s = speed * 1 / 1000f * 2;
           // Debug.Log("-----------------------------" + s);
            Ani.SetFloat("LaunchSpeed", 3 / s);
        }
    }
    /*
     * 5번 물줄기 6번 지진 이펙트를 만들어야함 별도의 prefeb으로 하고
     * 5번 물줄기의 경우는 그냥 단순한 공하나가 날아가는 걸로 (선형보간사용)
     * 6번 지진의 경우는 바닥 커지는 이펙트 그대로 사용 색만 바꿔서
     */
    public void SetTargetingEffectParent(ref GameObject target)
    {
        if (BossRokets) Destroy(BossRokets);
        BossRokets = Instantiate(BossTargetingEffectPrefab);
        BossRokets.GetComponent<TargetingEffect>().SetEffectParent(ref target);
    }
    public void BossWaterGunEffect(Vector3 start_pos, Vector3 end_pos, int speed)
    {
        GameObject go = Instantiate(BossWaterGunEffectPrefab, start_pos, Quaternion.identity);
        float s = speed * 1 / 1000f;
        //Debug.Log("effect w" + Time.time);
        go.GetComponent<WaterGunEffect>().speed = s;
        go.GetComponent<WaterGunEffect>().start_pos = start_pos;
        go.GetComponent<WaterGunEffect>().end_pos = end_pos;
    }

    public void BossTargetingEffect(Vector3 start_pos, ref GameObject target, int speed)
    {
        float s = speed * 1 / 1000f;

        BossRokets.GetComponent<TargetingEffect>().launch(ref target, s);
        //go.GetComponent<TargetingEffect>().start_pos = start_pos;
        //go.GetComponent<TargetingEffect>().target = target;
    }
    public void BossQuakeEffect(int startX, int startY, int startZ, int speed, HexDirection dir)
    {
        int up = (int)(dir - 1 < HexDirection.LeftUp ? HexDirection.RightDown : dir - 1);
        int mid = (int)dir;
        int down = (int)(dir + 1 > HexDirection.RightDown ? HexDirection.LeftUp : dir + 1);

        for (int i = 0; i < 5; ++i)
        {
            BossOneQuakeEffect(around[up, 0] * i + startX, around[up, 1] * i + startY, around[up, 2] * i + startZ, speed);
            BossOneQuakeEffect(around[mid, 0] * i + startX, around[mid, 1] * i + startY, around[mid, 2] * i + startZ, speed);
            BossOneQuakeEffect(around[down, 0] * i + startX, around[down, 1] * i + startY, around[down, 2] * i + startZ, speed);
        }
    }
    public void BossOneQuakeEffect(int startX, int startY, int startZ, int speed)
    {

        GameObject go = Instantiate(BossQuakeEffectPrefab, GameManager.data.grid.cellMaps.Get(startX, startY, startZ).getCellRealPosition(), Quaternion.identity);
        float s = speed * 1 / 1000f;
        //Debug.Log("effect q" + Time.time);
        go.GetComponent<QuakeEffect>().speed = s;
    }
    //// player attack


    //
    public void PlayerWaterGunEffect(Vector3 start_pos, ref GameObject target, int speed)
    {
        for (int i = 0; i < 5; ++i)
        {
            GameObject go = Instantiate(PlayerWaterGunEffectPrefab, start_pos, Quaternion.identity);
            float s = speed * 1 / 1000f;

            go.GetComponent<PlayerWaterGun>().Init(go.transform, ref target, s, 2);
        }
    }
    public void PlayerQuakeEffect(int startX, int startY, int startZ, int speed)
    {
        for (int i = 0; i < 6; ++i)
        {
            PlayerOneQuakeEffect(around[i, 0] + startX, around[i, 1] + startY, around[i, 2] + startZ, speed, i);

        }
    }
    public void PlayerOneQuakeEffect(int startX, int startY, int startZ, int speed, int dir)
    {

        GameObject go = Instantiate(PlayerQuakeEffectPrefab, GameManager.data.grid.cellMaps.Get(startX, startY, startZ).getCellRealPosition(), Quaternion.identity);
        float s = speed * 1 / 1000f;
        //Debug.Log("effect q" + Time.time);
        go.GetComponent<HexCellPosition>().setDirection((byte)dir);
        go.GetComponent<PlayerQuake>().speed = s;


    }

    public void PlayerHealEffect(int startX, int startY, int startZ, int speed)
    {
        //for (int i = 0; i < 6; ++i)
        //{
        //    PlayerOneHealEffect(around[i, 0] + startX, around[i, 1] + startY, around[i, 2] + startZ, speed);
        //}
        PlayerOneHealEffect(startX, startY, startZ, speed);
    }
    public void PlayerOneHealEffect(int startX, int startY, int startZ, int speed)
    {

        GameObject go = Instantiate(PlayerHealEffectPrefab, GameManager.data.grid.cellMaps.Get(startX, startY, startZ).getCellRealPosition(), Quaternion.identity);
        float s = speed * 1 / 1000f;
        //Debug.Log("effect q" + Time.time);
        go.GetComponent<PlayerHeal>().speed = s;
        //에니메이션 재생속도 조절
        //go.GetComponent<VisualEffect>().SetFloat = s;
    }
    IEnumerator CoOneTileWaveEffect(int x, int z, int power)
    {
        float t = 0f;
        float pret = 0f;
        int period = power / 2;
        while (t <= period)
        {
            pret = t;
            t += Time.deltaTime;
            float width = (period - t) * 0.2f;
            float displacement = width * Mathf.Sin(t * period * Mathf.PI) - width * Mathf.Sin(pret * period * Mathf.PI);
            //Debug.Log(Mathf.Sin(t*2));
            if (GameManager.data.grid.cellMaps.plusW(x, z, displacement) == false) yield break;

            yield return null;
        }
        GameManager.data.grid.cellMaps.SetW(x, z, 0);
    }
    IEnumerator CoTileRailWaveEffect(int startX, int startZ, int power, int dir)
    {
        float t = 1 / (float)power;

        int[,] around = new int[6, 3] {
        {0, -1, 1 },{ 1, -1, 0 }, { 1, 0, -1 },
        { 0, 1, -1 },{ -1, 1, 0 }, { -1, 0, 1 } };
        int step = 0;
        while (step < power)
        {
            step++;

            int nextX = startX + step * around[dir, 0];
            int nextZ = startZ + step * around[dir, 2];
            StartCoroutine(CoOneTileWaveEffect(nextX, nextZ, power));
            yield return new WaitForSecondsRealtime(t);
        }


    }
    IEnumerator CoTileWaveEffect(int startX, int startZ, int power)
    {
        float t = 1 / (float)power;
        int step = 0;
        int maxStep = power * 2 + 1;

        int[,] around = new int[6, 3] {
        {0, -1, 1 },{ 1, -1, 0 }, { 1, 0, -1 },
        { 0, 1, -1 },{ -1, 1, 0 }, { -1, 0, 1 } };

        int[,] range = new int[maxStep, maxStep];

        range[power, power] = 2;


        while (step < power)
        {

            step++;
            int state = step % 2 + 1;

            for (int i = power - step; i < maxStep; ++i)
            {
                for (int j = power - step; j < maxStep; ++j)
                {
                    if (range[i, j] == state)
                    {

                        //주변으로 파장 이동
                        for (int k = 0; k < 6; ++k)
                        {
                            int nextX = i + around[k, 0];
                            int nextZ = j + around[k, 2];
                            if (range[nextX, nextZ] == 0)
                            {
                                range[nextX, nextZ] = state % 2 + 1;
                                //위 아래 운동 코루틴 시작
                                StartCoroutine(CoOneTileWaveEffect(nextX - power + startX, nextZ - power + startZ, power));
                            }
                        }
                        range[i, j] = 3;
                    }


                }
            }

            yield return new WaitForSecondsRealtime(t);
        }

    }


    public void TileWaveEffect(int startX, int startZ, int power)
    {
        //Debug.Log(startX + ", " + startZ+", "+power);
        StartCoroutine(CoTileWaveEffect(startX, startZ, power));
    }

    public void TileRailWaveEffect(int startX, int startZ, int power, HexDirection dir)
    {
        StartCoroutine(CoTileRailWaveEffect(startX, startZ, power, (int)dir));
    }



    //Robot

    IEnumerable CoRobotLookAt(float speed, HexCoordinates end_pos)
    {
        float t = 0;
        while (t < speed)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // GameManager.data.enemy.GetComponent<HexCellPosition>().setDirection();

    }
    public void ikTest(HexCoordinates end_pos)
    {
        //GameManager.data.GetEnemyAnim().SetIKPosition();
    }
    public void JumpAttack(float speed, HexCoordinates end_pos, bool bAttack)
    {
        float s = speed * 1 / 1000f;
        StartCoroutine(CoJumpAttack(s, end_pos, bAttack));
    }
    IEnumerator CoJumpAttack(float speed, HexCoordinates end_pos, bool bAttack)
    {
        Animator Ani = GameManager.data.GetEnemyAnim();

        Vector3 look = GameManager.data.enemy.transform.forward;
        Vector3 targetLook = end_pos.getRealPosition() - GameManager.data.enemy.transform.position;
        float Theta = Vector3.Angle(look, targetLook);


        int turnTime = (int)(Theta / 45);// 도는 애니메이션 시간 == 1

        float totalAnimTime = 3.0f + turnTime;
        const float JumpAnimTime = 1.2f;
        const float attackAnimTime = 1.8f;


        float s = speed / totalAnimTime ;


        GameManager.data.GetEnemyAnim().SetFloat("AttackSpeed", attackAnimTime / s);
        GameManager.data.GetEnemyAnim().SetFloat("JumpSpeed", JumpAnimTime / s);


        if (turnTime > 0)
        {
            //60 이상이면 돌아보고
            float direction = Vector3.Dot(Vector3.Cross(targetLook, look), Vector3.up);


            if (direction > 0)
            {
                //Left
                Ani.SetTrigger("TurnLeft");
                Ani.SetBool("angleArrive", false);
                Ani.SetFloat("TurnSpeed", 1 / (s / turnTime));
                float t = 0;

                Quaternion qu = GameManager.data.enemy.transform.rotation;
                float startAngle = qu.eulerAngles.y;
                float endAngle = (startAngle - Theta);
                //
                while (t < s * turnTime)
                {
                    t += Time.deltaTime;

                    //Debug.Log(qu);
                    qu = Quaternion.Euler(new Vector3(0, Mathf.Lerp(startAngle, endAngle, t / (s*turnTime)), 0));
                    GameManager.data.enemy.transform.rotation = qu;
                    yield return null;
                }
                Ani.SetBool("angleArrive", true);

            }
            else
            {
                //Right
                Ani.SetTrigger("TurnRight");
                Ani.SetBool("angleArrive", false);
                Ani.SetFloat("TurnSpeed", 1 / (s / turnTime));
                float t = 0;

                Quaternion qu = GameManager.data.enemy.transform.rotation;
                float startAngle = qu.eulerAngles.y;
                float endAngle = (startAngle + Theta);
                //
                while (t < s*turnTime)
                {
                    t += Time.deltaTime;


                    qu = Quaternion.Euler(new Vector3(0, Mathf.Lerp(startAngle, endAngle, t / (s*turnTime)), 0));
                    GameManager.data.enemy.transform.rotation = qu;
                    yield return null;
                }
                Ani.SetBool("angleArrive", true);

            }


        }
        Ani.SetTrigger("StartJump");

        // moveSpeed
        float arrivalTime =JumpAnimTime * (s);

        
        {
            float t = 0;
            while (t < arrivalTime * 0.4)
            {
                t += Time.deltaTime;

                yield return null;

            }
            t = 0;
            Vector3 startPos = GameManager.data.enemy.transform.position;
            Vector3 endPos = end_pos.getRealPosition();
            while (t < arrivalTime * 0.6)
            {
                //내가 원하는 방향으로 이동..
                t += Time.deltaTime;

                GameManager.data.enemy.transform.position = Vector3.Slerp(startPos, endPos, t / (arrivalTime * 0.6f));
                yield return null;
            }
            GameManager.data.enemy.GetComponent<HexCellPosition>().SetPosition(end_pos.X, end_pos.Y, end_pos.Z);
            GameManager.data.enemy.GetComponent<HexCellPosition>().reflectPosition();

            if (bAttack == false)
            {
                GameManager.data.GetEnemyAnim().SetInteger("JumpAttackType", 3);
            }
            else
            {
                int type = Random.Range(1, 3);
                if (type == 1)
                    GameManager.data.GetEnemyAnim().SetInteger("JumpAttackType", 1);
                else if (type == 2)
                    GameManager.data.GetEnemyAnim().SetInteger("JumpAttackType", 2);

            }
            t = 0;
            while (t < arrivalTime * 0.4)
            {
                //내가 원하는 방향으로 이동..
                t += Time.deltaTime;

                //GameManager.data.enemy.transform.position = Vector3.Slerp(startPos, endPos, t / (arrivalTime * 0.6f));
                yield return null;
            }
            TileWaveEffect(end_pos.X, end_pos.Z, 5);

            // GameManager.data.GetEnemyAnim().SetBool("Jumping", false);
        }
    }

    public void GunAttack(float speed, HexCoordinates target_pos)
    {

        //타겟방향으로 돌아보는건 ik랑 애니메이션 활용해서 만드는걸로하고 일단 이펙트만 출력하자
       // Debug.Log(target_pos.getRealPosition());
        GameObject go = Instantiate(BossWaterGunEffectPrefab, target_pos.getRealPosition(), Quaternion.identity);
        float s = speed * 1 / 1000f;
        //Debug.Log("effect w" + Time.time);
        go.GetComponent<GunEffect>().speed = s;
    }

    public void ElectricBallEffect(Vector3 start_pos, ref GameObject target, int speed)
    {

        GameObject go = Instantiate(BossTargetingEffectPrefab, start_pos, Quaternion.identity);
        float s = speed * 1 / 1000f;

        go.GetComponent<TargetingEffect2>().Init(go.transform, target.transform, s, 2);

    }

}
