using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orgel : MonoBehaviour
{
    public static Orgel instance;
    public Animator anim;
    //private bool take;

    public bool isOrgelPlaying;

    private void Awake()
    {
        instance = this;
        //take = false;
    }

    public void Touch(int itemNum)
    {
        if (itemNum == 99)
        {
            Invoke("EndSound",1.2f);
            anim.SetTrigger("End");
            //take = false;
        }
        else
        {
            anim.SetTrigger("Open");
            //take = true;
            Invoke("PlayerSound",1f);
        }
        // if (take)
        // {
        //     Invoke("EndSound",1.2f);
        //     anim.SetTrigger("End");
        //     take = false;
            
        // }
        // else
        // {
        //     anim.SetTrigger("Open");
        //     take = true;
        //     Invoke("PlayerSound",1f);
            
        // }
    }

    public void PlayerSound()
    {
        isOrgelPlaying = true;
        SoundManager.instance.ambPlayer.Play();
        FieldGameManager.data.MN.ChangeMusicName(FieldGameManager.data.soundManager.getSongName(true));
    }

    public void EndSound()
    {
        isOrgelPlaying = false;
        SoundManager.instance.ambPlayer.Stop();
    }

    

}
