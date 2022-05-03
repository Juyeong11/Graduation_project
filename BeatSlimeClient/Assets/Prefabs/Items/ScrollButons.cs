using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollButons : MonoBehaviour
{
    public TMPro.TMP_Text n;
    public TMPro.TMP_Text s;
    byte v;
    int vs;

    public void Start()
    {
        init();
        Debug.Log("byte : " + v);
    }

    public void init()
    {
        v = byte.Parse(n.text);
        vs = int.Parse(s.text);
    }

    public void Press()
    {
        Network.SendUseItemPacket(v);
        PlayerPrefs.SetInt("inventory" + v, vs - 1);
        s.text = (vs - 1).ToString();
        vs -= 1;

        if (vs <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
