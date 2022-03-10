using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeJudgeEffect : MonoBehaviour
{
    public ParticleSystem judgeParticle;
    
    public void ParticleApply(JudgeCases c)
    {
        switch(c)
        {
            case JudgeCases.PERFECT:
                judgeParticle.Emit(12);
                //judgeParticle.Play();
                //judgeParticle.Play();
                break;
            case JudgeCases.GOOD:
                judgeParticle.Emit(6);
                //judgeParticle.Play();
                break;
            case JudgeCases.BAD:

                judgeParticle.Play();
                break;
        }
    }
}
