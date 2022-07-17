using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngamePartyUISetter : MonoBehaviour
{
    public ClassImageObject CIO;
    public Image classImage;
    public TMP_Text nameText;

    public Image HPImage;
    public Image prevHPImage;
    public Image CoolTime;

    public float cooltime;
    public float nowCooltime = 0;
    
    public void Set(int cid, string nameT)
    {
        classImage.sprite = CIO.ClassSprites[cid];
        //print("player name : " + nameT);
        nameText.text = nameT;
    }

    public void Update()
    {
        if (nowCooltime > 0)
        {
            nowCooltime -= Time.deltaTime;
        }
        CoolTime.fillAmount = nowCooltime / cooltime;
    }
}
