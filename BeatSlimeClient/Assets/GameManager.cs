using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager data;

    public PlayerManager player;

    public SoundManager soundManager;
    public SoundEffectManager soundEffectManager;
    public SoundProgressBar soundProgressBar;

    public float beatTimes = 1;
    public float beatCounter = 0;

    public bool isGameStart;

    void Start()
    {
        data = this;
        isGameStart = false;
    }

    public void PlaySound()
    {
        if (!isGameStart)
        {
            soundManager.PlayBGM("BAD_SEC");
            beatCounter = beatTimes = 60f / soundManager.GetBGMBpm("BAD_SEC");
            isGameStart = true;
        }
    }

    void FixedUpdate()
    {
        if (isGameStart)
        {
            beatCounter -= Time.deltaTime;
            if (beatCounter <= 0)
            {
                beatCounter = beatTimes;
                soundEffectManager.BeatEffect();
            }
        }
    }
}
