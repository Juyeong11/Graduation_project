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
        // 따로 클래스가 빠져있는 이유 : PlayerManager에서 하면 한 플레이어라도 죽으면 화면에 게임 오버가 뜨기 때문.
        if (!player.HP.isAlive)
        {
            gameObject.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.4f);
            t.SetActive(true);
        }
    }
}
