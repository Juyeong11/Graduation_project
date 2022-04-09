using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    

    GameObject Target;
    public GameObject Prefeb;
    public GameObject rf;
    // Start is called before the first frame update
    void Start()
    {
        Prefeb.GetComponent<WaterGunEffect>().start_pos = new Vector3(0, 0, 0);
        Prefeb.GetComponent<WaterGunEffect>().end_pos = new Vector3(10, 10, 10);
        Prefeb.GetComponent<WaterGunEffect>().speed = 1000;

        rf = Instantiate(Prefeb);
        SetTarget(ref rf);
    }
    public void SetTarget(ref GameObject target)
    {
        Target = target;
    }
    // Update is called once per frame
    void Update()
    {
        if(Target)
            Debug.Log(Target.transform.position.x + " " + Target.transform.position.y + " " + Target.transform.position.z);
    }
}
