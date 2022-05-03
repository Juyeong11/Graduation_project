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
        skillImage.sprite = CIO.ClassSprites[FieldPlayerManager.self_skillnum]; 
        Name.text = FieldPlayerManager.myName;
        Money.text = FieldPlayerManager.money.ToString();
    }
}
