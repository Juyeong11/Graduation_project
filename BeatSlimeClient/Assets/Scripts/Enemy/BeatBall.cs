using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatBall : MonoBehaviour
{
    bool On = false;

    Beat StartBeat;
    Beat DestinationBeatOffset;

    float StartTime;

    EnemyManager From;
    PlayerManager Destination;
    
    public void Init(Beat b, Beat i, GameObject from, GameObject dest)
    {
        On = true;
        StartBeat = b;
        DestinationBeatOffset = i;
        StartTime = Time.time;

        gameObject.transform.position = from.transform.position;
        gameObject.SetActive(true);

        From = from.GetComponent<EnemyManager>();
        Destination = dest.GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (On)
        {
            gameObject.transform.position = vecLerp(From.transform.position + new Vector3(0, (Time.time - StartTime)*2f + 1.5f, 0),
                                                                    new Vector3(Destination.transform.position.x, 0.3f, Destination.transform.position.z),
                                                                   (Time.time - StartTime) / (DestinationBeatOffset.GetBeatTime()*0.001f));

            if (Time.time - StartTime > DestinationBeatOffset.GetBeatTime() * 0.001f)
            { 
                gameObject.SetActive(false);
                On = false;
            }
        }

    }

    Vector3 vecLerp(Vector3 a, Vector3 b, float t)
    {
        //Debug.Log("t : " + t);
        Vector3 tmp = new Vector3();

        tmp.x = Mathf.Lerp(a.x, b.x, t);
        tmp.y = Mathf.Lerp(a.y, b.y, t);
        tmp.z = Mathf.Lerp(a.z, b.z, t);

        return tmp;
    }

}
