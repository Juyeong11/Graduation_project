using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public ShopPrices shopPrices;

    FieldPlayerManager PM;

    public TMP_Text[] skill_price;
    public TMP_Text[] skill_level;

    private int selectedskillNum;
    void Update()
    {
        skill_price[0].text = shopPrices.Skill1Prices[PM.skillLevelsContainer[0]].ToString();
        skill_price[1].text = shopPrices.Skill2Prices[PM.skillLevelsContainer[1]].ToString();
        skill_price[2].text = shopPrices.Skill3Prices[PM.skillLevelsContainer[2]].ToString();
        skill_level[0].text = "Lv. " + (PM.skillLevelsContainer[0]+1);
        skill_level[1].text = "Lv. " + (PM.skillLevelsContainer[1]+1);
        skill_level[2].text = "Lv. " + (PM.skillLevelsContainer[2]+1);
    }

    public void ShopOpen(FieldPlayerManager pm)
    {
        if (!PM)
            PM = pm;

        skill_price[0].text = shopPrices.Skill1Prices[PM.skillLevelsContainer[0]].ToString();
        skill_price[1].text = shopPrices.Skill2Prices[PM.skillLevelsContainer[1]].ToString();
        skill_price[2].text = shopPrices.Skill3Prices[PM.skillLevelsContainer[2]].ToString();
        skill_level[0].text = "Lv. " + (PM.skillLevelsContainer[0]+1);
        skill_level[1].text = "Lv. " + (PM.skillLevelsContainer[1]+1);
        skill_level[2].text = "Lv. " + (PM.skillLevelsContainer[2]+1);

        gameObject.SetActive(true);
    }

    public void ShopClose()
    {
        gameObject.SetActive(false);
    }
    
    public void BuySkill(int skillNum)
    {
        FieldGameManager.Net.SendBuyPacket((byte)((skillNum-1)*4+PM.skillLevelsContainer[skillNum-1]));
    }
}
