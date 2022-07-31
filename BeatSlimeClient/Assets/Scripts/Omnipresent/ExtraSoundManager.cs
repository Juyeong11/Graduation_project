using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESound
{
    Jump1,
    Jump2,
    Jump3,
    Jump4,
    Jump5,
    Point1,
    Point2,
    Question,
    Popup,
    Popdown,
    Fire,
    OK,
    NO,

    Star,


    Attack,
}

public class ExtraSoundManager : MonoBehaviour
{
    public static ExtraSoundManager instance;
    public EXTRASound Sound;
    bool mute = false;

    AudioSource Aud;

    void Awake()
    {
        instance = this;
        Aud = gameObject.GetComponent<AudioSource>();
    }

    public void SFX(ESound type)
    {
        switch (type)
        {
            case ESound.Jump1:
                Aud.clip = Sound.Sounds[0];
                break;
            case ESound.Jump2:
                Aud.clip = Sound.Sounds[1];
                break;
            case ESound.Jump3:
                Aud.clip = Sound.Sounds[2];
                break;
            case ESound.Jump4:
                Aud.clip = Sound.Sounds[3];
                break;
            case ESound.Jump5:
                Aud.clip = Sound.Sounds[4];
                break;
            case ESound.Point1:
                Aud.clip = Sound.Sounds[5];
                break;
            case ESound.Point2:
                Aud.clip = Sound.Sounds[6];
                break;
            case ESound.Question:
                Aud.clip = Sound.Sounds[7];
                break;
            case ESound.Popup:
                Aud.clip = Sound.Sounds[8];
                break;
            case ESound.Popdown:
                Aud.clip = Sound.Sounds[9];
                break;
            case ESound.Fire:
                Aud.clip = Sound.Sounds[10];
                break;
            case ESound.OK:
                Aud.clip = Sound.Sounds[11];
                break;
            case ESound.NO:
                Aud.clip = Sound.Sounds[12];
                break;
            case ESound.Star:
                Aud.clip = Sound.Sounds[13];
                break;
            case ESound.Attack:
                Aud.clip = Sound.Sounds[14];
                break;
                
        }
        Aud.pitch = Random.Range(0.9f, 1.1f);
        Aud.volume = 0.7f;

        if (mute)
            Aud.volume = 0f;
        Aud.Play();
    }

    public void JumpSound(float volume)
    {
        int r = Random.Range(0,5);
        switch (r)
        {
            case 0:
                SFX(ESound.Jump1);
                break;
            case 1:
                SFX(ESound.Jump2);
                break;
            case 2:
                SFX(ESound.Jump3);
                break;
            case 3:
                SFX(ESound.Jump4);
                break;
            case 4:
                SFX(ESound.Jump5);
                break;
        }
        Aud.pitch = Random.Range(0.9f, 1.1f);
        Aud.volume = volume;

        if (mute)
            Aud.volume = 0f;
        Aud.Play();
    }

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.F8))
    //     {
    //         mute = !mute;
    //     }
    //     if (Input.GetKeyDown(KeyCode.Keypad1))
    //     {
    //         SFX(ESound.Jump1);
    //     }
    //     if (Input.GetKeyDown(KeyCode.Keypad2))
    //     {
    //         SFX(ESound.Jump2);
    //     }
    //     if (Input.GetKeyDown(KeyCode.Keypad3))
    //     {
    //         SFX(ESound.Jump3);
    //     }
    //     if (Input.GetKeyDown(KeyCode.Keypad4))
    //     {
    //         SFX(ESound.Jump4);
    //     }
    //     if (Input.GetKeyDown(KeyCode.Keypad5))
    //     {
    //         SFX(ESound.Jump5);
    //     }
    //     if (Input.GetKeyDown(KeyCode.Keypad6))
    //     {
    //         SFX(ESound.Point1);
    //     }
    //     if (Input.GetKeyDown(KeyCode.Keypad7))
    //     {
    //         SFX(ESound.Point2);
    //     }
    //     if (Input.GetKeyDown(KeyCode.Keypad8))
    //     {
    //         SFX(ESound.Question);
    //     }
    //     if (Input.GetKeyDown(KeyCode.Keypad9))
    //     {
    //         SFX(ESound.Popup);
    //     }
    //     if (Input.GetKeyDown(KeyCode.Keypad0))
    //     {
    //         SFX(ESound.Popdown);
    //     }
    // }
}
