using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "TutorialMapOBJs", menuName = "Scriptable Object/TutorialMapOBJs", order = int.MaxValue)]
public class TutorialMapOBJs : ScriptableObject
{
    [SerializeField]
    private List<Sprite> alertImage;

    [SerializeField]
    private List<HEXCOORD> triggerPosition;


    public List<Sprite> AlertImage { get { return alertImage; } }

    public List<HEXCOORD> TriggerPosition { get { return triggerPosition; } }

}

[System.Serializable]
public class HEXCOORD
{
    public int x;
    public int z;
}