using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
public class TileEffect2 : MonoBehaviour
{
    public float speed;
    VisualEffect vfWarning;
    VisualEffect vfAirStrike;
    const float warningLifeTime = 2.0f;
    const float airStrikeLiftTime = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void initialize(float spd)
    {
        VisualEffect[] temp = GetComponentsInChildren<VisualEffect>();
        vfWarning = temp[0];
        vfAirStrike = temp[1];
        vfWarning.Stop();
        vfAirStrike.Stop();
        speed = spd;
        StartCoroutine(Animation());
    }


    // Update is called once per frame
    IEnumerator Animation()
    {
        vfWarning.Play();
        vfWarning.playRate = 1/((1/ warningLifeTime) * speed);


        //playRate == 1 => playTime == 2
        //playTime == speed
        float t = 0.0f;
        while (t <= speed - 0.5)
        {
            t += Time.deltaTime;
            yield return null;
        }

        t = 0;
        vfAirStrike.Play();
        while (t <= 1)
        {
            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
