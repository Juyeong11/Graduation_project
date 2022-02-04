using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverImage : MonoBehaviour
{
    public PlayerManager player;
    public GameObject t;
    void Start()
    {
        t.SetActive(false);
    }

    void Update()
    {
        if (!player.HP.isAlive)
        {
            gameObject.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.4f);
            t.SetActive(true);
        }
    }
}
