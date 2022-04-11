using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SlimeDataPanel : MonoBehaviour
{
    public ClassImageObject CIO;
    public Image skillImage;
    public TMP_Text Name;
    public TMP_Text Money;

    void Update()
    {
        //print(PlayerPrefs.GetInt("mySkill",0));
        skillImage.sprite = CIO.ClassSprites[PlayerPrefs.GetInt("mySkill",0)]; 
        Name.text = PlayerPrefs.GetString("myName","");
        Money.text = PlayerPrefs.GetInt("Money",0).ToString();
    }
}
