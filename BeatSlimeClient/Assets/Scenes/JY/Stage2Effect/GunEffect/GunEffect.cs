using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class GunEffect : MonoBehaviour
{
    //총구 화염 -> 원래는 enemyManager에 있어야하지만 이거 마녀도 같이 쓰는 거라 손을 댈수가 없네 -> 상속을 쓰면 되지
    //쏘는 애니메이션 -> 쏘는 애니메이션이 특정 프레임이 되면 총구 화염 이펙트를 출력하도록 만들어야함
    //피격 이펙트
    //이거 3개가 speed시간에 완료돼야함
    public float speed = 0;

    VisualEffect vfWarning;
    VisualEffect vfShoot1;
    VisualEffect vfShoot2;
    VisualEffect vfHit1;
    VisualEffect vfHit2;

    const float lifeTime = 2;
    // Start is called before the first frame update
    void Start()
    {


        VisualEffect[] tmp = GetComponentsInChildren<VisualEffect>();
        Debug.Log(tmp.Length);
        vfWarning = tmp[0];
        vfShoot1 = tmp[1];
        vfShoot2 = tmp[2];
        vfHit1 = tmp[3];
        vfHit2 = tmp[4];

        vfWarning.Stop();
        vfShoot1.Stop();

        vfShoot2.Stop();
        vfHit1.Stop();
        vfHit2.Stop();

        GameManager.data.enemy.GetComponentInChildren<GunEffect2>().targetPos = transform.position;
        StartCoroutine(Animation());
    }
    public void Run()
    {
       
    }
   
    // Update is called once per frame
    IEnumerator Animation()
    {

       

        vfWarning.playRate = 1 / (1 / lifeTime * speed*0.8f);
        vfWarning.Play();

        //여기서 돌아보고
        float t = 0;
        while (t < speed*0.8f)
        {
            t += Time.deltaTime;
            yield return null;

        }

        //슛
        Animator Ani = GameManager.data.GetEnemyAnim();
        Ani.SetTrigger("StartShoot");
        Ani.SetBool("Shooting",true);


        t = 0;
        while (t < speed*0.2f)
        {
            t += Time.deltaTime;

          

            yield return null;
        }
        Ani.SetBool("Shooting",false);
        Destroy(gameObject);
    }
}
