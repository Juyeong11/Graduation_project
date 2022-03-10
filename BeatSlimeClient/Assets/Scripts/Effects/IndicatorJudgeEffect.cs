using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum JudgeCases
{
    PERFECT,
    GOOD,
    BAD
}

public class IndicatorJudgeEffect : MonoBehaviour
{
    public Image judgeImage;
    public List<Sprite> judgeSprites;

    float countee = 0;
    bool perfected = false;

    // Update is called once per frame
    public void Update()
    {
        if (judgeImage.color.a > 0f)
        {
            judgeImage.color = new Color(judgeImage.color.r, judgeImage.color.g, judgeImage.color.b, judgeImage.color.a - (2.0f * Time.deltaTime));
            countee += 0.5f;
            if (perfected)
                judgeImage.sprite = judgeSprites[((int)countee)%4];
        }
    }

    public void JudgeApply(JudgeCases c)
    {
        switch(c)
        {
            case JudgeCases.PERFECT:
                perfected = true;
                judgeImage.color = new Color(judgeImage.color.r, judgeImage.color.g, judgeImage.color.b, 1f);
                break;
            case JudgeCases.GOOD:
                judgeImage.sprite = judgeSprites[4];
                perfected = false;
                judgeImage.color = new Color(judgeImage.color.r, judgeImage.color.g, judgeImage.color.b, 1f);
                break;
            case JudgeCases.BAD:
                judgeImage.sprite = judgeSprites[5];
                perfected = false;
                judgeImage.color = new Color(judgeImage.color.r, judgeImage.color.g, judgeImage.color.b, 1f);
                break;
        }
    }
    
}
