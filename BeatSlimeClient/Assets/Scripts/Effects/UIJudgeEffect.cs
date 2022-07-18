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

    public Image sM;
    public Image sC;
    public Image sH;
    public Image sT;
    public Image sO;

    public List<Sprite> comboSprites;
    public List<Sprite> scoreSprites;

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
        if (alpp > 0.0f)
        {
            alpp -= 1.0f * Time.deltaTime;
            HHH.color = new Color(1.0f, 1.0f, 1.0f, alpp);
            if (TTT.color.a > 0.0f)
            {
                TTT.color = new Color(1.0f, 1.0f, 1.0f, alpp);
            }
            if (OOO.color.a > 0.0f)
            {
                OOO.color = new Color(1.0f, 1.0f, 1.0f, alpp);
            }
            VVV.color = new Color(1.0f, 1.0f, 1.0f, alpp);
            
            if (alpp < 0.0f)
            {
                alpp = 0.0f;
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

    public void ScoreApply(int score)
    {
        if (score<0)
            score = 0;
        sT.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        sH.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        sC.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        sM.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        sO.sprite = scoreSprites[score % 10];
        score /= 10;
        if (score > 0)
        {
            sT.sprite = scoreSprites[score%10];
            score /= 10;
            sT.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            if (score > 0)
            {
                sH.sprite = scoreSprites[score%10];
                score /= 10;
                sH.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                if (score > 0)
                {
                    sC.sprite = scoreSprites[score%10];
                    score /= 10;
                    sC.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    if (score > 0)
                    {
                        sM.sprite = scoreSprites[score%20];
                        sM.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    }
                }
            }
        }
    }
}
