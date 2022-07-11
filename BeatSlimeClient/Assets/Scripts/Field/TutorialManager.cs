using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public Image tutorialImage;
    public TutorialMapOBJs tutorialMapOBJs;
    
    public PlayerManager PM;
    
    public void Update()
    {
        for (int i=0;i<tutorialMapOBJs.AlertImage.Count;i++)
        {
            if (PM.selfCoord.coordinates.X == tutorialMapOBJs.TriggerPosition[i].x && PM.selfCoord.coordinates.Z == tutorialMapOBJs.TriggerPosition[i].z)
            {
                tutorialImage.sprite = tutorialMapOBJs.AlertImage[i];
                break;
            }
        }
    }
}
