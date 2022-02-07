using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    static public EffectManager instance;
    public GameObject TileEffectPrefab;
    public GameObject QuakeEffectPrefab;
    public GameObject WaterGunEffectPrefab;
    public GameObject TargetingEffectPrefab;


    public GameObject PlayerAttackEffectPrefab;

    int[,] around = new int[6, 3] { { 1, -1, 0 }, { 1, 0, -1 }, { 0, 1, -1 }, { -1, 1, 0 }, { -1, 0, 1 }, { 0, -1, 1 } };

    Effects HexPattern;

    // Start is called before the first frame update
    void Start()
    {
        HexPattern = new Effects();
        instance = this;
    }

    Vector3 GetHexCellPosition(int x, int y, int z, int w)
    {
        if (x + y + z != 0) Debug.Log("x+y+z != 0");
        return new Vector3(x * 0.866f, w + 0.2f, x * 0.5f + z * 1f);
    }
    // 1방향 공격
    public void OneTileEffect(int start_x, int start_y, int start_z, int speed)
    {

        GameObject go = Instantiate(TileEffectPrefab, GameManager.data.grid.cellMaps.Get(start_x, start_y, start_z).getCellRealPosition(), Quaternion.identity);
        float s = speed * 1 / 1000f;
        //Debug.Log("effect t" + Time.time);
        go.GetComponent<TileEffect>().speed = s;
    }
    public void OneQuakeEffect(int start_x, int start_y, int start_z, int speed)
    {

        GameObject go = Instantiate(QuakeEffectPrefab, GameManager.data.grid.cellMaps.Get(start_x, start_y, start_z).getCellRealPosition(), Quaternion.identity);
        float s = speed * 1 / 1000f;
        //Debug.Log("effect q" + Time.time);
        go.GetComponent<QuakeEffect>().speed = s;
    }
    public void OneWaterGunEffect(Vector3 start_pos, Vector3 end_pos, int speed)
    {
        GameObject go = Instantiate(WaterGunEffectPrefab, start_pos, Quaternion.identity);
        float s = speed * 1 / 1000f;
        //Debug.Log("effect w" + Time.time);
        go.GetComponent<WaterGunEffect>().speed = s;
        go.GetComponent<WaterGunEffect>().start_pos = start_pos;
        go.GetComponent<WaterGunEffect>().end_pos = end_pos;
    }

    public void BossTileEffect(int start_x, int start_y, int start_z, int speed, int EffectType)
    {
        foreach (var pat in HexPattern.GetPattern(EffectType))
        {
            OneTileEffect(pat.x + start_x, pat.y + start_y, pat.z + start_z, speed);
        }

    }
    /*
     * 5번 물줄기 6번 지진 이펙트를 만들어야함 별도의 prefeb으로 하고
     * 5번 물줄기의 경우는 그냥 단순한 공하나가 날아가는 걸로 (선형보간사용)
     * 6번 지진의 경우는 바닥 커지는 이펙트 그대로 사용 색만 바꿔서
     */
    public void BossWaterGunEffect(Vector3 start_pos,Vector3 end_pos, int speed)
    {
        OneWaterGunEffect(start_pos, end_pos, speed);
    }
    public void BossTargetingEffect(Vector3 start_pos,ref GameObject target, int speed)
    {
        GameObject go = Instantiate(TargetingEffectPrefab, start_pos, Quaternion.identity);
        float s = speed * 1 / 1000f;
        //Debug.Log("effect w" + Time.time);
        go.GetComponent<TargetingEffect>().speed = s;
        go.GetComponent<TargetingEffect>().start_pos = start_pos;
        go.GetComponent<TargetingEffect>().target = target;
    }
    public void BossQuakeEffect(int start_x, int start_y, int start_z, int speed, HexDirection dir)
    {
        int up = (int)(dir - 1 < HexDirection.LeftUp ? HexDirection.RightDown : dir - 1);
        int mid = (int)dir;
        int down = (int)(dir + 1 > HexDirection.RightDown ? HexDirection.LeftUp : dir + 1);

       for (int i = 0; i < 5; ++i)
        {
            OneQuakeEffect(around[up, 0]*i + start_x, around[up, 1]*i + start_y, around[up, 2]*i + start_z, speed);
            OneQuakeEffect(around[mid, 0]*i + start_x, around[mid, 1]*i + start_y, around[mid, 2]*i + start_z, speed);
            OneQuakeEffect(around[down, 0]*i + start_x, around[down, 1]*i + start_y, around[down, 2]*i + start_z, speed);
        }
    }
    //// 6방향 공격
    //public void BossTileEffect4(int start_x, int start_y, int start_z, int start_w, int speed)
    //{
    //    for (int i = 0; i < 8; ++i)
    //    {
    //        OneTileEffect(HexPattern4[i, 0] + start_x, HexPattern4[i, 1] + start_y, HexPattern4[i, 2] + start_z, start_w, speed);
    //    }
    //}

    public void PlayerAttackEffect0(int start_x, int start_y, int start_z, int start_w)
    {
        for (int dir = 0; dir < 6; ++dir)
            Instantiate(PlayerAttackEffectPrefab, GetHexCellPosition(around[(int)dir, 0] + start_x, around[(int)dir, 1] + start_y, around[(int)dir, 2] + start_z, start_w), Quaternion.identity);
    }
}
