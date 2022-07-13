using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class GunEffect2 : MonoBehaviour
{
    public VisualEffect vfShoot1;
    public VisualEffect vfShoot2;
    public VisualEffect vfHit1;
    public VisualEffect vfHit2;

    public Vector3 targetPos;
    // Start is called before the first frame update

    void Start()
    {



        vfShoot1.Stop();
        vfShoot2.Stop();
        vfHit1.Stop();
        vfHit2.Stop();

    }
    void Update()
    {

    }
    public void Shoot1()
    {

        Debug.Log(targetPos);
        vfShoot1.Play();
        vfHit1.transform.position = targetPos + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        vfHit1.Play();
    }
    public void Shoot2()
    {



        vfShoot2.Play();
        vfHit2.transform.position = targetPos + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        vfHit2.Play();
    }


}
