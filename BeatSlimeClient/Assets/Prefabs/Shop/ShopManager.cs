using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class ShopManager : MonoBehaviour
{
    const int SKILL_MAX_LEVEL = 3;
    public ShopPrices shopPrices;

    public TMP_Text[] skill_price;
    public TMP_Text[] skill_level;

    private int selectedskillNum;
    void Update()
    {
        if (FieldPlayerManager.skillLevelsContainer[0]==SKILL_MAX_LEVEL)
        {
            skill_price[0].text = " ";
            skill_level[0].text = "LV. MAX";
        }
        else
        {
            skill_price[0].text = shopPrices.Skill1Prices[FieldPlayerManager.skillLevelsContainer[0]+1].ToString();
            skill_level[0].text = "Lv. " + (FieldPlayerManager.skillLevelsContainer[0]+1);
        }
        if (FieldPlayerManager.skillLevelsContainer[1] == SKILL_MAX_LEVEL)
        {
            skill_price[1].text = " ";
            skill_level[1].text = "LV. MAX";
        }
        else
        {
            skill_price[1].text = shopPrices.Skill2Prices[FieldPlayerManager.skillLevelsContainer[1]+1].ToString();
            skill_level[1].text = "Lv. " + (FieldPlayerManager.skillLevelsContainer[1] + 1);
        }
        if (FieldPlayerManager.skillLevelsContainer[2] == SKILL_MAX_LEVEL)
        {
            skill_price[2].text = " ";
            skill_level[2].text = "LV. MAX";
        }
        else
        {
            skill_price[2].text = shopPrices.Skill3Prices[FieldPlayerManager.skillLevelsContainer[2]+1].ToString();
            skill_level[2].text = "Lv. " + (FieldPlayerManager.skillLevelsContainer[2] + 1);
        }
    }

    public void ShopOpen()
    {
        if (FieldPlayerManager.skillLevelsContainer[0]==SKILL_MAX_LEVEL)
        {
            skill_price[0].text = " ";
            skill_level[0].text = "LV. MAX";
        }
        else
        {
            skill_price[0].text = shopPrices.Skill1Prices[FieldPlayerManager.skillLevelsContainer[0]+1].ToString();
            skill_level[0].text = "Lv. " + (FieldPlayerManager.skillLevelsContainer[0]+1);
        }
        if (FieldPlayerManager.skillLevelsContainer[1] == SKILL_MAX_LEVEL)
        {
            skill_price[1].text = " ";
            skill_level[1].text = "LV. MAX";
        }
        else
        {
            skill_price[1].text = shopPrices.Skill2Prices[FieldPlayerManager.skillLevelsContainer[1]+1].ToString();
            skill_level[1].text = "Lv. " + (FieldPlayerManager.skillLevelsContainer[1] + 1);
        }
        if (FieldPlayerManager.skillLevelsContainer[2] == SKILL_MAX_LEVEL)
        {
            skill_price[2].text = " ";
            skill_level[2].text = "LV. MAX";
        }
        else
        {
            skill_price[2].text = shopPrices.Skill3Prices[FieldPlayerManager.skillLevelsContainer[2]+1].ToString();
            skill_level[2].text = "Lv. " + (FieldPlayerManager.skillLevelsContainer[2] + 1);
        }

        gameObject.SetActive(true);
    }

    public void ShopClose()
    {
        gameObject.SetActive(false);
    }
    
    public void BuySkill(int skillNum)
    {
        if (FieldPlayerManager.skillLevelsContainer[skillNum-1] == SKILL_MAX_LEVEL)
        {
            return;
        }
        //Debug.Log($"Skill Buy : {skillNum}");
        Network.SendBuyPacket((byte)((skillNum-1)*4+FieldPlayerManager.skillLevelsContainer[skillNum-1]+1));
    }
}
