using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillConveter : MonoBehaviour
{
    public Animator[] rings;

    public void SkillConvet()
    {
        FieldPlayerManager.self_skillnum++;
        if (FieldPlayerManager.self_skillnum > 3)
        {
            FieldPlayerManager.self_skillnum = 1;
        }
        rings[FieldPlayerManager.self_skillnum - 1].SetTrigger("Skill");
        Network.SendChangeSkillPacket((byte)FieldPlayerManager.self_skillnum);
    }

    public void SetSkill(int n)
    {
        FieldPlayerManager.self_skillnum = n;
        rings[n - 1].SetTrigger("Skill");
        Network.SendChangeSkillPacket((byte)FieldPlayerManager.self_skillnum);
    }
}
