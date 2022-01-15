using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPool : MonoBehaviour
{

    public static EffectPool instance;


    public Queue<GameObject> PlayerObjectQueue = new Queue<GameObject>();
    public Queue<GameObject> EnemyObjectQueue = new Queue<GameObject>();


    public GameObject PlayerPrefeb;
    public GameObject TileEffectPrefeb;
    public GameObject EnemyPrefeb;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        PlayerObjectQueue = InsertQueue(10, TileEffectPrefeb, null);
    }

    Queue<GameObject> InsertQueue(int count, GameObject prefeb, Transform tr)
    {
        Queue<GameObject> t_queue = new Queue<GameObject>();
        for (int i = 0; i < count; ++i)
        {
            GameObject t_clone = Instantiate(prefeb);
            t_clone.SetActive(false);
            if (tr != null)
                t_clone.transform.SetParent(tr);
            else
                t_clone.transform.SetParent(this.transform);

            t_queue.Enqueue(t_clone);
        }
        return t_queue;
    }
}
