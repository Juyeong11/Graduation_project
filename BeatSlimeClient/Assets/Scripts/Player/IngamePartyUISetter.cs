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
    
    public void Set(int cid, string nameT)
    {
        classImage.sprite = CIO.ClassSprites[cid];
        //print("player name : " + nameT);
        nameText.text = nameT;
    }
}
