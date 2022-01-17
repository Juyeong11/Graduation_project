using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEffect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Animation());
    }


    // Update is called once per frame
    void Update()
    {
        //Objects[pid].GetComponent<HexCellPosition>().SetPosition(p.x, p.y, p.z);
    }
    IEnumerator Animation()
    {
        float scale = 0.0f;
        while(scale <= 1.0f)
        {
            scale += 0.25f*Time.deltaTime; 
            gameObject.transform.localScale = new Vector3(scale, 0.01f, scale);
            yield return null;
        }
        Destroy(gameObject);
    }
}
