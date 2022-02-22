using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWaterGun : MonoBehaviour
{
    public float speed;
    public Vector3 start_pos;
    public GameObject target;
    // Start is called before the first frame update


    public Vector3 pos1;
    public Vector3 pos2;
    private float CubicBezierCurve(float a, float b, float c, float d, float t)
    {
        return Mathf.Pow((1 - t), 3) * a
            + Mathf.Pow((1 - t), 2) * 3 * t * b
            + Mathf.Pow(t, 2) * 3 * (1 - t) * c
            + Mathf.Pow(t, 3) * d;
    }

    public void Init(Transform start, ref GameObject end,float s, float randomDistance)
    {
        start_pos = start.position;
        target = end;
        speed = s;

        pos1 = start.position +
        (randomDistance * Random.Range(-1.0f, 1.0f) * start.right) +
        (randomDistance * Random.Range(-0.15f, 1.0f) * start.up) +
        (randomDistance * Random.Range(-1.0f, -0.8f) * start.forward);

        pos2 = end.transform.position +
        (randomDistance * Random.Range(-1.0f, 1.0f) * end.transform.right) +
        (randomDistance * Random.Range(-1.0f, 1.0f) * end.transform.up) +
        (randomDistance * Random.Range(-0.8f, -0.8f) * end.transform.forward);

        StartCoroutine(Animation());
    }
    IEnumerator Animation()
    {
        float scale = 0.0f;
        gameObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        while (scale <= 1.0f)
        {
            scale += Time.deltaTime * 1 / speed;
            

            transform.position = new Vector3(
                CubicBezierCurve(start_pos.x, pos1.x, pos2.x, target.transform.position.x, scale),
                CubicBezierCurve(start_pos.y, pos1.y, pos2.y, target.transform.position.y, scale),
                CubicBezierCurve(start_pos.z, pos1.z, pos2.z, target.transform.position.z, scale)
                );

            yield return null;
        }
        //wait trail
        gameObject.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);

        scale = 0.0f;
        while (scale <= 1.0f)
        {
            scale += Time.deltaTime * 1 / speed;
            yield return null;
        }
        Destroy(gameObject);
    }
}
