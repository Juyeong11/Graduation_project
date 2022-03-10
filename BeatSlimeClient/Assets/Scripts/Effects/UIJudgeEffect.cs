using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIJudgeEffect : MonoBehaviour
{
    public Image H;
    public Image T;
    public Image O;

    public List<Sprite> comboSprites;

    int countee = 0;
    float scale = 2.0f;

    public void Update()
    {
        if (scale > 2.0f)
        {
            scale -= 1.0f * Time.deltaTime;
            H.transform.localScale = new Vector3(scale, scale, scale);
            T.transform.localScale = new Vector3(scale, scale, scale);
            O.transform.localScale = new Vector3(scale, scale, scale);

            if (scale < 2.0f)
                scale = 2.0f;
        }
    }

    public void CountApply(ref int combo, bool crush = false)
    {
        combo++;
        if (crush)
            combo = 0;
        scale = 2.2f;
        H.sprite = comboSprites[(combo / 100) + (10*(countee%4))];
        T.sprite = comboSprites[((combo%100) / 10) + (10*(countee%4))];
        O.sprite = comboSprites[((combo%10)) + (10*(countee%4))];

        H.transform.localScale = new Vector3(scale, scale, scale);
        T.transform.localScale = new Vector3(scale, scale, scale);
        O.transform.localScale = new Vector3(scale, scale, scale);
        countee++;
    }
}
