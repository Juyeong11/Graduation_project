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

    public Text bt;
    public Text M;
    public Text ts;

    public Text money;
    public Text scroll_grade;


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
            M.text = "F";
            Center.SetActive(true);
        }
        else if (gameEnder == GameEndTraits.Win)
        {
            t.text = "Game Clear!";
            M.text = "A";
            Center.SetActive(true);
        }
    }

    public void SetGameEnd(GameEndTraits end)
    {
        gameEnder = end;

    }
    public void SetResultData(int perfect, int great, int miss, int attack, int damaged, int score, int mone, int scroll_grad)
    {
        bt.text = perfect + "\n" + great + "\n" + miss + "\n\n" + attack + "\n" + damaged;
        ts.text = score.ToString();
        money.text = mone.ToString();
        FieldPlayerManager.money += mone;
        
        scroll_grade.text = scroll_grad.ToString();
        PlayerPrefs.SetInt("inventory" + scroll_grad, PlayerPrefs.GetInt("inventory" + scroll_grad) + 1);
    }
}
