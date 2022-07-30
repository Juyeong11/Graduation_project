using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FadeIOManager : MonoBehaviour
{
    public Image FIP;
    public Image FII;

    public Image FOP;
    public Image FOI;

    int FadeState;

    public UnityEvent ChangeSceneEvent;

    public float FadeSpeed = 1;

    void Awake()
    {
        FOP.color = new Color(0,0,0,1);
        FOI.fillAmount = 1;

        FIP.color = new Color(0,0,0,0);
        FII.fillAmount = 0;

        FadeState = -1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            FadeState = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            FadeState = -1;
        }

        if (FadeState == 0)
        {
            return;
        }
        else if (FadeState == -1)
        {
            if (FOI.fillAmount > 0)
                FOI.fillAmount -= Time.deltaTime * FadeSpeed;
            else
                FOP.color = new Color(0,0,0,FOP.color.a - (Time.deltaTime * FadeSpeed));
            if (FOP.color.a < 0)
            {
                FadeState = 0;
            }
        }
        else if (FadeState == 1)
        {
            if (FIP.color.a < 1)
                FIP.color = new Color(0,0,0,FIP.color.a + (Time.deltaTime * FadeSpeed));
            else
                FII.fillAmount += Time.deltaTime * FadeSpeed;
            if (FII.fillAmount > 1)
            {
                FadeState = 0;
                ChangeSceneEvent.Invoke();
            }
        }
    }
}
