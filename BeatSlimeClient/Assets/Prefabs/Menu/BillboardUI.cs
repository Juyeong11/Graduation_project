using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BillboardUI : MonoBehaviour
{
    private Transform Target;
    public TMP_Text NAME;
    public Sprite join;
    public Sprite exit;
    public Image pops;

    [System.NonSerialized]
    public int p;

    void Update()
    {
     	transform.position = Vector3.Lerp(transform.position, Target.position, Time.deltaTime * 2f);
    }

    public void GetOff()
    {
        gameObject.SetActive(true);
        StartCoroutine(BlinkOff());
    }

    public void GetOn(Transform t, int pid = -1)
    {
        gameObject.SetActive(true);
        Target = t;
        p = pid;
        if (pid == -1)
        {
            NAME.text = PlayerPrefs.GetString("myName");
            pops.sprite = exit;
        }
        else
        {
            NAME.text = t.parent.GetComponent<FieldOtherPlayerManager>().other_playerName;
            pops.sprite = join;
        }
        StartCoroutine(BlinkOn());
    }

    IEnumerator BlinkOn()
    {
        
        while (transform.localScale.x < 1f)
        {
            transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
            yield return new WaitForSeconds(0.01f);
        }
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    IEnumerator BlinkOff()
    {
        while (transform.localScale.x > 0)
        {
            transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
            yield return new WaitForSeconds(0.01f);
        }

        gameObject.SetActive(false);
        yield return null;
    }

    public void SendPartyRequest()
    {
        if (p == -1)
        {
            FieldGameManager.Net.SendPartyRequestAnwserPacket(0, FieldGameManager.myPlayerID);
            PartyManager.instance.DelParty();
        }
        else
        {
            FieldGameManager.Net.SendPartyRequestPacket(p);
        }

        GetOff();
    }
}
