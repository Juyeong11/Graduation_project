using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameEndTraits {
    None,
    Win,
    Lose
}

public class GameOverImage : MonoBehaviour
{
    public GameEndTraits gameEnder;
    public Text t;
    public GameObject Center;


    void Start()
    {
        gameEnder = GameEndTraits.None;
        Center.SetActive(false);
    }

    void Update()
    {
        // 따로 클래스가 빠져있는 이유 : PlayerManager에서 하면 한 플레이어라도 죽으면 화면에 게임 오버가 뜨기 때문.
        if (gameEnder == GameEndTraits.Lose)
        {
            t.text = "Game Over";
            Center.SetActive(true);
        }
        else if (gameEnder == GameEndTraits.Win)
        {
            t.text = "Game Clear!";
            Center.SetActive(true);
        }
    }

    public void SetGameEnd(GameEndTraits end)
    {
        gameEnder = end;
    }
}
