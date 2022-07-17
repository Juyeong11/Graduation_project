using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIJudgeEffect : MonoBehaviour
{
    public Image H;
    public Image T;
    public Image O;

    public Image HH;
    public Image TT;
    public Image OO;

    public List<Sprite> comboSprites;
    public List<Sprite> damageSprites;

    int countee = 0;
    float scale = 2.0f;
    float alp = 0.0f;

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

        if (alp > 0.0f)
        {
            alp -= 1.0f * Time.deltaTime;
            HH.color = new Color(1.0f, 1.0f, 1.0f, alp);
            TT.color = new Color(1.0f, 1.0f, 1.0f, alp);
            OO.color = new Color(1.0f, 1.0f, 1.0f, alp);
            if (alp < 0.0f)
            {
                alp = 0.0f;
            }
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

    public void DamageApply(int damage)
    {
        alp = 1.5f;
        TT.sprite = damageSprites[damage/10];
        OO.sprite = damageSprites[damage%10];

        HH.color = new Color(1.0f, 1.0f, 1.0f, alp);
        TT.color = new Color(1.0f, 1.0f, 1.0f, alp);
        OO.color = new Color(1.0f, 1.0f, 1.0f, alp);
    }
}
