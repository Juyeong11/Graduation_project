using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
public class TileEffect2 : MonoBehaviour
{
    public float speed;
    VisualEffect vfWarning;
    VisualEffect vfAirStrike;
    const float lifeTime = 2.0f;
    // Start is called before the first frame update
    void Start()
    {
        
        vfWarning = GetComponent<VisualEffect>();
        vfWarning.Stop();
        vfAirStrike = GetComponentInChildren<VisualEffect>();
        vfAirStrike.Stop();
        StartCoroutine(Animation());
    }


    // Update is called once per frame
    IEnumerator Animation()
    {
        vfWarning.Play();
        vfWarning.playRate = 1/(0.5f * speed);
  

        //playRate == 1 => playTime == 2
        //playTime == speed
        float t = 0.0f;
        while (t <= speed)
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
