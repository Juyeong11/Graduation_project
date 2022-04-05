using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicName : MonoBehaviour
{
    string nowMusicName;
    public Text t;

    public void ChangeMusicName(string musicName)
    {
        nowMusicName = musicName;
        StartCoroutine(deleteMusicName());

        t.text = "";
    }

    char randomChar()
    {
        int r = Random.Range(0, 26);
        char c = (char)('a' + r);
        return c;
    }

    IEnumerator deleteMusicName()
    {
        string nName = nowMusicName;

        yield return fillMusicName();

        while (nName.Length > 0)
        {
            for (int i=0;i<5;i++)
            {
                nName = randomChar() + nName.Substring(1);
                t.text = nName;
                yield return new WaitForSeconds(0.01f);
            }
            nName = nName.Substring(1);
        }
        nName = "";
        t.text  = "";
    }

    IEnumerator fillMusicName()
    {
        int ind = 0;
        while (ind < nowMusicName.Length)
        {

            for (int i=0;i<5;i++)
            {
                t.text = randomChar() + nowMusicName.Substring(nowMusicName.Length - ind);
                yield return new WaitForSeconds(0.01f);
            }
            ind++;
        }
        t.text = nowMusicName;
        
        yield return new WaitForSeconds(1f);


    }
}
