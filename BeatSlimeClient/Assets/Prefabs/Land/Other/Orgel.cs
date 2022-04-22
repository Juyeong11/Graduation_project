using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orgel : MonoBehaviour
{
    public static Orgel instance;
    public Animator anim;
    private bool take;
    private void Awake()
    {
        instance = this;
        take = false;
    }

    public void Touch()
    {
        if (take)
        {
            Invoke("EndSound",1f);
            anim.SetTrigger("End");
            take = false;
        }
        else
        {
            anim.SetTrigger("Open");
            take = true;
            Invoke("PlayerSound",1f);
        }
    }

    public void PlayerSound()
    {
        SoundManager.instance.ambPlayer.Play();
        FieldGameManager.data.MN.ChangeMusicName(FieldGameManager.data.soundManager.getSongName(true));
    }

    public void EndSound()
    {

        SoundManager.instance.ambPlayer.Stop();
    }

    

}
