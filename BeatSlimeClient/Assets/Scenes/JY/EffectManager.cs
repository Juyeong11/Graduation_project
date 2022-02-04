using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    static public EffectManager instance;
    public GameObject TileEffectPrefab;
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
        //Debug.Log(s);
        go.GetComponent<TileEffect>().speed = s;
    }

    public void BossTileEffect(int start_x, int start_y, int start_z, int speed, int EffectType)
    {
        foreach (var pat in HexPattern.GetPattern(EffectType))
        {
            OneTileEffect(pat.x + start_x, pat.y + start_y, pat.z + start_z, speed);
        }

    }

    //public void BossTileEffect3(int start_x, int start_y, int start_z, int start_w, int speed)
    //{

    //    for (int i = 0; i < 10; ++i)
    //    {
    //        OneTileEffect(HexPattern3[i, 0] + start_x, HexPattern3[i, 1] + start_y, HexPattern3[i, 2] + start_z, start_w, speed);
    //    }
    //}
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
