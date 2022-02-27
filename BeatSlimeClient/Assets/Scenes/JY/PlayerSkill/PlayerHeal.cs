using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeal : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Animation());
    }
    IEnumerator Animation()
    {
        float scale = 0.0f;
        while (scale <= 1.0f)
        {
            scale += Time.deltaTime * 1 / speed;
            //gameObject.transform.localScale = new Vector3(scale, 0.01f, scale);
            yield return null;
        }
        Destroy(gameObject);
    }
}
