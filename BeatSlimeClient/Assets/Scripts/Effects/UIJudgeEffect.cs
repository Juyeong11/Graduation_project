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

    public Image HHH;
    public Image TTT;
    public Image OOO;
    public Image VVV;

    public List<Sprite> comboSprites;

    int countee = 0;
    float scale = 2.0f;
    float alp = 0.0f;
    float alpp = 0.0f;

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
        TT.sprite = comboSprites[(damage/10) + 30];
        OO.sprite = comboSprites[(damage%10) + 30];


        HH.color = new Color(1.0f, 1.0f, 1.0f, alp);
        if (damage/10 == 0)
        {
            TT.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
        else
        {
            TT.color = new Color(1.0f, 1.0f, 1.0f, alp);
        }


        OO.color = new Color(1.0f, 1.0f, 1.0f, alp);
    }

    public void AttackApply(int damage)
    {
        alpp = 1.5f;
        TTT.sprite = comboSprites[(damage/100) + 10];
        OOO.sprite = comboSprites[((damage/10)%10) + 10];
        VVV.sprite = comboSprites[(damage%10) + 10];

        HHH.color = new Color(1.0f, 1.0f, 1.0f, alpp);
        if (damage/100 == 0)
        {
            TTT.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
        else
        {
            TTT.color = new Color(1.0f, 1.0f, 1.0f, alpp);
        }

        //이거 맞음
        if (damage/10 == 0)
        {
            OOO.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
        else
        {
            OOO.color = new Color(1.0f, 1.0f, 1.0f, alpp);
        }

        VVV.color = new Color(1.0f, 1.0f, 1.0f, alpp);
    }
}
