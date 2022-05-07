using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WaterGunEffect : MonoBehaviour
{
    public float speed;
    public Vector3 start_pos;

    public Vector3 end_pos;

    // Start is called before the first frame update

    private void Start()
    {
        StartCoroutine(Animation());
    }
    IEnumerator Animation()
    {
        float scale = 0.0f;
        VisualEffect vf = GetComponentInChildren<VisualEffect>();
        start_pos.y += 1.8f;
        transform.position = start_pos;
        //vf.SetFloat("Lifetime", 0.1f);
        Debug.Log("Length" + vf.GetFloat("Length"));
        Vector3 lookVec = end_pos - start_pos;
        vf.SetFloat("Length", lookVec.magnitude*0.0651f);
        Debug.Log("lookVec " + lookVec.magnitude);
        Debug.Log("vf.playRate" + vf.playRate);
        vf.playRate = 5;
        vf.Play();

        //end_pos.z *= -1.0f;
        transform.LookAt(end_pos);
        while (scale <= 10.0f)
        {
            scale += Time.deltaTime * 1 / speed;
            //gameObject.transform.localScale = new Vector3(scale, scale, scale);

            //포물선 이동
            //Vector3 cur_pos = Vector3.Lerp(start_pos, end_pos, scale);
            //cur_pos.y *=  (1+Mathf.Sin(Mathf.Lerp(0,3.14f,scale)));
            //transform.position = cur_pos;

            yield return null;
        }
        Destroy(gameObject);
    }
}
