using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillConveter : MonoBehaviour
{
    public Animator[] rings;
    public FieldPlayerManager PM;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            PM.self_skillnum++;
            if (PM.self_skillnum > 3)
            {
                PM.self_skillnum = 1;
            }
            rings[PM.self_skillnum - 1].SetTrigger("Skill");
            FieldGameManager.Net.SendChangeSkillPacket((byte)PM.self_skillnum);
        }
    }
}
