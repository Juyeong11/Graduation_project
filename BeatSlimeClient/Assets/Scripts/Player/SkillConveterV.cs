using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillConveterV : MonoBehaviour
{
    public Animator[] rings;

    public void SetSkill(int n)
    {
        rings[n - 1].SetTrigger("Skill");
    }
}
