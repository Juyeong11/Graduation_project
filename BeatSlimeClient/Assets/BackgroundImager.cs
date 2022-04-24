using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundImager : MonoBehaviour
{
    public Camera main;
    float selfX;
    float selfY;

     void Start()
    {
        selfX = gameObject.GetComponent<RectTransform>().anchoredPosition.x;
        selfY = gameObject.GetComponent<RectTransform>().anchoredPosition.y;

    }

    void Update()
    {
        gameObject.GetComponent<RectTransform>().anchoredPosition =
       Vector3.Lerp(gameObject.GetComponent<RectTransform>().anchoredPosition,
        new Vector3(selfX - main.transform.position.x*100f, selfY + main.transform.position.y*100f, 0), Time.deltaTime * 2f);
    }
}
