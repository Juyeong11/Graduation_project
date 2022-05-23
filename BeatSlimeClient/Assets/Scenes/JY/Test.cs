using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    

   
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(OneTileWaveEffect(0,0,1));
    }
    IEnumerator OneTileWaveEffect(int x, int z, int power)
    {
        float t = 0.0f;
        // power가 쌔지면 파동크기와 주기가 커지게? 이건 너무 심심하다
        //일단 그냥 진행ㄱ
        while (t <= 1.0f)
        {

            t += Time.deltaTime;


            GameManager.data.grid.cellMaps.SetW(x, z, Mathf.Sin(t));

            yield return null;
        }

    }
}
