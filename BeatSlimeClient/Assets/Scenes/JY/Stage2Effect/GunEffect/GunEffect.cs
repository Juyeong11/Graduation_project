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

        GameManager.data.enemy.GetComponentInChildren<AnimEvent>().targetPos = transform.position;
        StartCoroutine(Animation());
    }
    public void Run()
    {

    }

    // Update is called once per frame
    IEnumerator Animation()
    {
        vfWarning.playRate = 1 / (1 / lifeTime * speed);
        vfWarning.Play();
        Animator Ani = GameManager.data.GetEnemyAnim();


        // 보스가 보는 방향과 내 위치 각도 구해서 30으로 나눠서 어느 방향으로 돌지 정하자
        Vector3 look = GameManager.data.enemy.transform.forward;
        Vector3 targetLook = transform.position - GameManager.data.enemy.transform.position;
        float Theta = Vector3.Angle(look, targetLook);
        if(Theta > 60)
        {
            Ani.SetBool("Shooting", false);
            //60 이상이면 돌아보고
            float direction = Vector3.Dot(Vector3.Cross(targetLook, look), Vector3.up);
            int cnt = (int)(Theta / 45);

            Debug.Log(direction);
            if (direction > 0)
            {
                //Left
                Ani.SetTrigger("TurnLeft");
                Ani.SetBool("angleArrive", false);
                Ani.SetFloat("TurnSpeed", 1 / (speed/cnt));
                float t = 0;

                Quaternion qu = GameManager.data.enemy.transform.rotation;
                float startAngle = qu.eulerAngles.y;
                float endAngle = (startAngle - Theta);
                //
                while (t < speed)
                {
                    t += Time.deltaTime;

                    
                    //Debug.Log(qu);
                    qu = Quaternion.Euler(new Vector3(0, Mathf.Lerp(startAngle, endAngle, t/speed),0));
                    GameManager.data.enemy.transform.rotation = qu;
                    yield return null;
                }
                Ani.SetBool("angleArrive", true);

            }
            else
            {
                //Right
                Ani.SetTrigger("TurnRight");
                Ani.SetBool("angleArrive", false);
                Ani.SetFloat("TurnSpeed", 1 / (speed / cnt));
                float t = 0;

                Quaternion qu = GameManager.data.enemy.transform.rotation;
                float startAngle = qu.eulerAngles.y;
                float endAngle = (startAngle + Theta);
                //
                while (t < speed)
                {
                    t += Time.deltaTime;


                    //Debug.Log(qu);
                    qu = Quaternion.Euler(new Vector3(0, Mathf.Lerp(startAngle, endAngle, t / speed), 0));
                    GameManager.data.enemy.transform.rotation = qu;
                    yield return null;
                }
                Ani.SetBool("angleArrive", true);

            }

        }
        else
        {
            float t = 0;
            while (t < speed)
            {
                t += Time.deltaTime;
                yield return null;

            }

        }

    

        //슛

        Ani.SetTrigger("StartShoot");
        Ani.SetBool("Shooting", true);
        {
            float t =0;
            
            while (t < speed)
            {
                t += Time.deltaTime;



                yield return null;
            }
            Ani.SetBool("Shooting", false);
            Destroy(gameObject);
        }


    }
}
