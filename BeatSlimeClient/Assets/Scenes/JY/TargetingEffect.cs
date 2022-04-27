using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingEffect : MonoBehaviour
{

    const int roketCnt = 8;
    public GameObject Root;

    public GameObject RoketPrefab;
    public GameObject RoketMeshPrefab;

    GameObject[] RoketMesh = new GameObject[roketCnt];
    GameObject[] RoketEffect = new GameObject[roketCnt];
    GameObject[] targets = new GameObject[roketCnt];
    bool[] launchReady = new bool[roketCnt];
    float[] speed = new float[roketCnt];

    float diameter = 0.5f;
    float size = 0.3f;

    float offsetH = 0.0f;
    float offsetZ = 0.8f;

    public float a = 240.0f;
    // Start is called before the first frame update
    void OnEnable()
    {
        for (int i = 0; i < roketCnt; ++i)
        {

            GameObject r = Instantiate(RoketPrefab);
            GameObject rm = Instantiate(RoketMeshPrefab);

            Vector3 scale = r.transform.localScale;
            scale = scale * size;
            r.transform.localScale = scale;

            scale = rm.transform.localScale;
            scale = scale * size;
            rm.transform.localScale = scale;

            RoketMesh[i] = rm;
            RoketEffect[i] = r;
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
            RoketMesh[i].transform.position = pos;

            Vector3 roketForward = Root.transform.forward - Root.transform.up;
            RoketMesh[i].transform.forward = roketForward;
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
                RoketEffect[i].transform.position = RoketMesh[i].transform.position;
                RoketMesh[i].SetActive(false);
                RoketEffect[i].SetActive(true);
                StartCoroutine(RoketLauncher(i));
                break;
            }

    }
    public void ParryingNearRoket(Transform player)
    {
        float m = float.MaxValue;
        int index = 0;
        for (int i = 0; i < roketCnt; ++i)
        {
            if (launchReady[i] == true)
            {
                RoketEffect[i].transform.position = player.position;
                launchReady[i] = false;
                //RoketEffect[i].transform.position = RoketMesh[i].transform.position;

                RoketMesh[i].SetActive(false);
                RoketEffect[i].SetActive(true);
                StartCoroutine(ParryingAni(i));
                break;
            }
        }
    }
    IEnumerator ParryingAni(int index)
    {
        float t = 0.0f;

        Vector3 startPos = RoketEffect[index].transform.position;
        while (t <= 1.0f)
        {

            t += Time.deltaTime;
            Vector3 cur_pos = Vector3.Lerp(startPos, Root.transform.position, t);
            Vector3 dir = startPos - Root.transform.position;
            RoketEffect[index].transform.rotation = Quaternion.LookRotation(dir);//z축 뒤집혀있음
            RoketEffect[index].transform.position = cur_pos;
            yield return null;
        }
        RoketMesh[index].SetActive(true);
        RoketEffect[index].SetActive(false);
        targets[index] = null;
        speed[index] = 0;
        launchReady[index] = true;
    }
    IEnumerator RoketLauncher(int index)
    {
        float t = 0.0f;
        Vector3 start_Pos = RoketEffect[index].transform.position;
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
            RoketEffect[index].transform.rotation = Quaternion.LookRotation(-direction);//z축 뒤집혀있음
            //cur_pos.y *= (1 + Mathf.Sin(Mathf.Lerp(0, 3.14f, t)));
            RoketEffect[index].transform.position = cur_pos;
            yield return null;
        }
        RoketMesh[index].SetActive(true);
        RoketEffect[index].SetActive(false);
        targets[index] = null;
        speed[index] = 0;
        launchReady[index] = true;
    }

}
