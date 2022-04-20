using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectInfo
{
    public GameObject goPrefeb;
    public int count;
    public Transform tfPoolParent;
}
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;
    //static int nums = 1;

    public Queue<GameObject> PlayerObjectQueue = new Queue<GameObject>();
    public Queue<GameObject> EnemyObjectQueue = new Queue<GameObject>();


    public GameObject PlayerPrefeb;
    public GameObject OtherPlayerPrefeb;
    public GameObject EnemyPrefeb;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        PlayerObjectQueue = InsertQueue(Protocol.CONSTANTS.MAX_USER - 1, OtherPlayerPrefeb, null);
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
