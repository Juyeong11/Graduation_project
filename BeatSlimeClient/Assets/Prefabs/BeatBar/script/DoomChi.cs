using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoomChi : MonoBehaviour
{
    public RectTransform spinner;
    public Image selfImage;
    public Image selftag;
    public GameObject self;
    public Animator anim;
    private float BPM;
    private float firstTime;

    public void Update()
    {
        if (!anim.enabled)
        {
            float b = spinner.transform.localEulerAngles.z % 90;
            if (b > 70f)
            {
                gameObject.transform.localScale = new Vector3(0.2f + ((-70f + b) * 0.002f), 0.2f + ((-70f + b) * 0.002f), 1f);
            }
            else
            {
                gameObject.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
            }
        }
    }

    public void SetStart(float BPM, float Time)
    {
        anim.enabled = false;
        selftag.color = new Color(selftag.color.r, selftag.color.g, selftag.color.b, 1f);
        selfImage.color = new Color(selfImage.color.r, selfImage.color.g, selfImage.color.b, 1f);
        selfImage.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
        selftag.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
        firstTime = (60f / BPM) - Time % (60f / BPM);
        this.BPM = BPM;
        //StartCoroutine(doomchi());
    }

    IEnumerator doomchi()
    {
        anim.ResetTrigger("Beat");
        yield return new WaitForSeconds(firstTime);
        anim.SetTrigger("Beat");
        yield return new WaitForSeconds(60f / BPM);
        anim.ResetTrigger("Beat");
        anim.SetTrigger("Beat");
        yield return new WaitForSeconds(60f / BPM);
        anim.ResetTrigger("Beat");
        anim.SetTrigger("Beat");
    }

    public void noteEnd()
    {
        anim.enabled = true;
        anim.SetTrigger("End");
    }

    void dead()
    {
        anim.ResetTrigger("End");
        anim.ResetTrigger("Perfect");
        anim.ResetTrigger("Good");
        anim.ResetTrigger("Beat");

        StartCoroutine(real());
    }

    public void notePerfect()
    {
        anim.enabled = true;
        anim.SetTrigger("Perfect");
    }

    public void noteGood()
    {
        anim.enabled = true;
        anim.SetTrigger("Good");
    }

    IEnumerator real()
    {
        gameObject.transform.position = new Vector3(1000f, 1000f,0f);
        anim.SetTrigger("Q");
        yield return new WaitForSeconds(2f);
        anim.ResetTrigger("Q");
        gameObject.SetActive(false);
    }
}
