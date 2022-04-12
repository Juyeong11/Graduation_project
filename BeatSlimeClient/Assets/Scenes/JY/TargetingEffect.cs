using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingEffect : MonoBehaviour
{

    const int roketCnt = 8;
    public GameObject Root;

    public GameObject RoketPrefab;
    GameObject[] Roket = new GameObject[roketCnt];
    GameObject[] targets = new GameObject[roketCnt];
    bool[] launchReady = new bool[roketCnt];
    float[] speed = new float[roketCnt];

    float diameter = 0.5f;
    float size = 0.3f;

    float offsetH = 1.44f;
    float offsetZ = 0.8f;

    public float a = 240.0f;
    // Start is called before the first frame update
    void OnEnable()
    {
        for (int i = 0; i < roketCnt; ++i)
        {

            GameObject go = Instantiate(RoketPrefab);

            Vector3 scale = go.transform.localScale;
            scale = scale * size;
            go.transform.localScale = scale;

            Roket[i] = go;
            launchReady[i] = true;
        }

        //StartCoroutine(Animation());
    }
    public void SetEffectParent(ref GameObject p)
    {
        Root = p;

        //StartCoroutine(RotateAnimation());
    }
    private void Update()
    {
        if (Root == null) return;
        Vector3 origin = Root.transform.position;

        // origin.z *= -1; 
        Quaternion q = Quaternion.LookRotation(Root.transform.forward);
        //origin = q * origin;

        for (int i = 0; i < roketCnt; ++i)
        {
            if (launchReady[i] == false) continue;
            float length = diameter;
            float angle = (((i+1) * 240.0f / (roketCnt)) - 40);
            float CosTheta = Mathf.Cos(angle * Mathf.Deg2Rad);
            float SinTheta = Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector3 pos = new Vector3(length * CosTheta, length * SinTheta + offsetH, offsetZ);

            pos = q * pos;
            pos += origin;
            Roket[i].transform.position = pos;

            Vector3 roketForward = Root.transform.forward - Root.transform.up;
            Roket[i].transform.forward = roketForward;
        }

    }
    public void launch(ref GameObject p, float s)
    {
        for (int i = 0; i < roketCnt; ++i)
            if (launchReady[i] == true)
            {
                targets[i] = p;
                launchReady[i] = false;
                speed[i] = s;

                StartCoroutine(RoketLauncher(i));
                break;
            }

    }
    public void ParryingNearRoket()
    {
        float m = float.MaxValue;
        int index = 0;
        for (int i = 0; i < roketCnt; ++i)
        {
            if (launchReady[i] == true)
            {
                Roket[i].transform.position = GameManager.data.player.transform.position;
                launchReady[i] = false;

                StartCoroutine(ParryingAni(i));
                break;
            }
        }
    }
    IEnumerator ParryingAni(int index)
    {
        float t = 0.0f;

        Vector3 startPos = Roket[index].transform.position;
        while (t <= 1.0f)
        {

            t += Time.deltaTime;
            Vector3 cur_pos = Vector3.Lerp(startPos, Root.transform.position, t);
            Vector3 dir = startPos - Root.transform.position;
            Roket[index].transform.rotation = Quaternion.LookRotation(dir);//z축 뒤집혀있음
            Roket[index].transform.position = cur_pos;
            yield return null;
        }
        targets[index] = null;
        speed[index] = 0;
        launchReady[index] = true;
    }
    IEnumerator RoketLauncher(int index)
    {
        float t = 0.0f;
        Vector3 start_Pos = Roket[index].transform.position;
        Vector3 p1 = Root.transform.position;
        p1.y += 5.0f;
        p1.x += Random.Range(-5.0f, 5.0f);
        p1.z += Random.Range(-5.0f, 5.0f);
        while (t <= 1.0f)
        {
            t += Time.deltaTime * 1 / speed[index];

            Vector3 p2 = targets[index].transform.position;

            Vector3 p01 = Vector3.Lerp(start_Pos, p1, t);
            Vector3 p02 = Vector3.Lerp(p1, p2, t);

            Vector3 cur_pos = Vector3.Lerp(p01, p02, t);
            Vector3 direction = p02 - p01;
            Roket[index].transform.rotation = Quaternion.LookRotation(-direction);//z축 뒤집혀있음
            //cur_pos.y *= (1 + Mathf.Sin(Mathf.Lerp(0, 3.14f, t)));
            Roket[index].transform.position = cur_pos;
            yield return null;
        }
        targets[index] = null;
        speed[index] = 0;
        launchReady[index] = true;
    }

}
