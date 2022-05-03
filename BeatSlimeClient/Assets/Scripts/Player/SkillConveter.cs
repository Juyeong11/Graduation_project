using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillConveter : MonoBehaviour
{
    public Animator[] rings;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            FieldPlayerManager.self_skillnum++;
            if (FieldPlayerManager.self_skillnum > 3)
            {
                FieldPlayerManager.self_skillnum = 1;
            }
            rings[FieldPlayerManager.self_skillnum - 1].SetTrigger("Skill");
            FieldGameManager.Net.SendChangeSkillPacket((byte)FieldPlayerManager.self_skillnum);
        }
    }
}
