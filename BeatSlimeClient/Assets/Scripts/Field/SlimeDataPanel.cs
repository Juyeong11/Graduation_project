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
        skillImage.sprite = CIO.ClassSprites[FieldPlayerManager.instance.self_skillnum]; 
        Name.text = FieldPlayerManager.instance.myName;
        Money.text = FieldPlayerManager.instance.money.ToString();
    }
}
