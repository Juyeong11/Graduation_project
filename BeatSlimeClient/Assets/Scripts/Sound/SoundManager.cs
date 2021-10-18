using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SfxSound
{
    public string name;
    public AudioClip clip;
}

[System.Serializable]
public class BgmSound
{
    public string name;
    public AudioClip clip;
    public int bpm;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] SfxSound[] sfx = null;
    [SerializeField] BgmSound[] bgm = null;

    [SerializeField] AudioSource bgmPlayer = null;
    [SerializeField] AudioSource[] sfxPlayer = null;

    float pAnimUnitBPM = 45f;
    public Animator pAnimation;
    public Animator eAnimation;
    // Start is called before the first frame update

    private void Awake()
    {
        instance = this;
    }
    public void PlayBGM(string p_bgmName)
    {
        for (int i = 0; i < bgm.Length; ++i)
        {
            if (p_bgmName == bgm[i].name)
            {
                pAnimation.SetFloat("Speed", bgm[i].bpm / pAnimUnitBPM);
                bgmPlayer.clip = bgm[i].clip;
                bgmPlayer.Play();
            }
        }
    }
    public int GetBGMBpm(string p_bgmName)
    {
        for (int i = 0; i < bgm.Length; ++i)
        {
            if (p_bgmName == bgm[i].name)
            {
                return bgm[i].bpm;
            }
        }
        return 0;
    }

    public float GetMusicProgress()
    {
        return (bgmPlayer.time / bgmPlayer.clip.length);
    }

    public float GetMusicLength(string songName)
    {
        for (int i = 0; i < bgm.Length; ++i)
        {
            if (songName == bgm[i].name)
            {
                return bgm[i].clip.length;
            }
        }
        return 0;
    }
    public float GetMusicLength()
    {
        return bgmPlayer.clip.length;
    }
    public int GetMusicLapsedTime()
    {
        return (int)(bgmPlayer.time * 1000);
    }

    public void StopBGM()
    {
        bgmPlayer.Stop();
    }

    public void PlaySFX(string p_sfxName)
    {
        for (int i = 0; i < sfx.Length; ++i)
        {
            if (p_sfxName == sfx[i].name)
            {
                for (int x = 0; x < sfxPlayer.Length; ++x)
                {
                    if (!sfxPlayer[i].isPlaying)
                    {
                        sfxPlayer[x].clip = sfx[i].clip;
                        sfxPlayer[x].Play();

                        return;
                    }
                }
                Debug.Log("모든 오디오 플레이어가 재생 중입니다.");
                return;

            }
        }
        Debug.Log("해당 이름을 가지는 오디오가 없습니다.");
    }
}
