using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TargetingEffect2 : MonoBehaviour
{
    VisualEffect vfElectircBall;

    public float speed;
    public Vector3 startPos;
    public Vector3 targetPos;

    public Vector3 pos1;
    public Vector3 pos2;
    private float CubicBezierCurve(float a, float b, float c, float d, float t)
    {
        return Mathf.Pow((1 - t), 3) * a
            + Mathf.Pow((1 - t), 2) * 3 * t * b
            + Mathf.Pow(t, 2) * 3 * (1 - t) * c
            + Mathf.Pow(t, 3) * d;
    }

    void Start()
    {

    }

    public void Init(Transform start, Transform end, float s, float randomDistance)
    {
        vfElectircBall = GetComponent<VisualEffect>();
        vfElectircBall.Stop();
        startPos = start.position;
        targetPos = end.position;
        speed = s;

        pos1 = start.position +
        (randomDistance * Random.Range(-1.0f, 1.0f) * start.right) +
        (randomDistance * Random.Range(1.0f, 2.0f) * start.up) +
        (randomDistance * Random.Range(-1.0f, -0.8f) * start.forward);

        pos2 = end.position +
        (randomDistance * Random.Range(-1.0f, 1.0f) * end.right) +
        (randomDistance * Random.Range(0.0f, 1.0f) * end.up) +
        (randomDistance * Random.Range(-0.8f, -0.8f) * end.forward);

        StartCoroutine(Animation());
    }
    // Update is called once per frame
    IEnumerator Animation()
    {
        gameObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        vfElectircBall.Play();
        float scale = 0.0f;
       // gameObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        while (scale <= 1.0f)
        {
            scale += Time.deltaTime * 1 / speed;

           
            transform.position = new Vector3(
                CubicBezierCurve(startPos.x, pos1.x, pos2.x, targetPos.x, scale),
                CubicBezierCurve(startPos.y, pos1.y, pos2.y, targetPos.y, scale),
                CubicBezierCurve(startPos.z, pos1.z, pos2.z, targetPos.z, scale)
                );
            //Debug.Log(transform.position);
            yield return null;
        }
        //패링 성공 애니메이션은 나중에
        Destroy(gameObject);
    }

}
