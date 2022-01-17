using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    static public EffectManager instance;
    public GameObject TileEffectPrefab;
    public GameObject PlayerAttackEffectPrefab;

    int[,] around = new int[6, 3] { { 1, -1, 0 }, { 1, 0, -1 }, { 0, 1, -1 }, { -1, 1, 0 }, { -1, 0, 1 }, { 0, -1, 1 } };
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }
    Vector3 GetHexCellPosition(int x, int y, int z, int w)
    {
        if (x + y + z != 0) Debug.Log("x+y+z != 0");
        return new Vector3(x * 0.866f, w + 0.2f, x * 0.5f + z * 1f);
    }
    // 1방향 공격
    public IEnumerator OneLineTileEffect(int start_x, int start_y, int start_z, int start_w, HexDirection dir)
    {
        float t = 0.0f;
        int MAX_LENGTH = 6;
        while (t < MAX_LENGTH)
        {
            t += 1;

            Instantiate(TileEffectPrefab, GetHexCellPosition(around[(int)dir, 0] * (int)t + start_x, around[(int)dir, 1] * (int)t + start_y, around[(int)dir, 2] * (int)t + start_z, start_w), Quaternion.identity);

            yield return new WaitForSeconds(1f);
        }

    }
    public void BossTileEffect0(int start_x, int start_y, int start_z, int start_w, HexDirection dir)
    {
        StartCoroutine(OneLineTileEffect(start_x, start_y, start_z, start_w, dir));
    }
    // 6방향 공격
    public void BossTileEffect1(int start_x, int start_y, int start_z, int start_w)
    {
        for (int i = 0; i < 6; ++i)
        {
            StartCoroutine(OneLineTileEffect(start_x, start_y, start_z, start_w, (HexDirection)i));
        }
    }

    public void PlayerAttackEffect0(int start_x, int start_y, int start_z, int start_w)
    {
        for (int dir = 0; dir < 6; ++dir)
            Instantiate(PlayerAttackEffectPrefab, GetHexCellPosition(around[(int)dir, 0] + start_x, around[(int)dir, 1] + start_y, around[(int)dir, 2] + start_z, start_w), Quaternion.identity);
    }
}
