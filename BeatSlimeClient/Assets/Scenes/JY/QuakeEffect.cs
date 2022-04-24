using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class QuakeEffect : MonoBehaviour
{
    public float speed;
    VisualEffect vf;

    // Start is called before the first frame update
    void Start()
    {
        vf = GetComponentInChildren<VisualEffect>();
        vf.Stop();
        StartCoroutine(Animation());
    }

    IEnumerator Animation()
    {
        float scale = 0.0f;
        while (scale <= 1.0f)
        {
            scale += Time.deltaTime * 1 / speed;
            gameObject.transform.GetChild(0).localScale = new Vector3(scale, 0.01f, scale);
            yield return null;
        }

        gameObject.transform.GetChild(0).localScale = new Vector3(0, 0, 0);
        //gameObject.transform.localScale = new Vector3(3, 3, 3);
        scale = 0.0f;

        vf.Play();

        while (scale <= 2.0f)
        {
            scale += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
