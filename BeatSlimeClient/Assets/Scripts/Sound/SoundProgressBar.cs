using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundProgressBar : MonoBehaviour
{
    RectTransform rectTransform;
    float WidthPer;
    // Start is called before the first frame update
    void Start()
    {
        WidthPer = Screen.width / 100.0f;
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Debug.Log(SoundManager.instance.GetMusicProgress());

        rectTransform.localScale = new Vector3(SoundManager.instance.GetMusicProgress(),1,1);

    }
}
