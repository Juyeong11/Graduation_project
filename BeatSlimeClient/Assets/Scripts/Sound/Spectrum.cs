using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Spectrum : MonoBehaviour
{
    public int CreateStickNum = 0;          //생성될 막대 개수
    public float interval = 0;              //생성될 막대 간격
    public List<GameObject> Sticks;         //GameObject 막대 생성

    public GameObject Yellow = null;        // 노란색 막대 선언

    void Awake()
    {
        for (int i = 0; i < CreateStickNum; i++)
        {
            GameObject obj = (GameObject)Instantiate(Yellow);                   //
            obj.transform.SetParent(GameObject.Find("Spectrum").transform, false);
            //obj.transform.parent = GameObject.Find("Spectrum").transform;       //  노란색 막대를 interval 만큼의 간격으로
            obj.transform.localScale = new Vector3(10f,10f);                             //  CreateStickNum개수만큼 생성
            obj.transform.localPosition = new Vector2(0 + interval * i, 0);     //
            obj.transform.localRotation = Quaternion.Euler(0,0,0);

            Sticks.Add(obj);                                                    // 생성된 막대를 list에 추가
        }
    }

    void Update()
    {
        //AudioMixer에서 Kick만 가져오면 GetSpectrum을 킥에서만 가능?
        //Mixer에서는 어떻게 가져오는지 모르겠는데 AudioSource에서는 가능함
        //멀티 트랙을 뽑아서 유니티에서 믹싱을 해야되나본데
        float[] SpectrumData = new float[1024];
        AudioListener.GetSpectrumData(SpectrumData, 0, FFTWindow.Hamming);          // 스펙트럼데이터 배열에 오디오가 듣고있는 스펙트럼데이터를 대입
        for (int i = 0; i < Sticks.Count; i++)
        {
            Vector2 FirstScale = Sticks[i].transform.localScale;                                    // 처음 막대기 스케일을 변수로 생성
            FirstScale.y = SpectrumData[i] * 1600;                                            // 막대기 y를 스펙트럼데이터에 맞게 늘림
            if (FirstScale.y > 85f)
                FirstScale.y = 85f;
            Sticks[i].transform.localScale = Vector2.MoveTowards(Sticks[i].transform.localScale, FirstScale, 20f);     // 스펙트럼데이터에 맞게 늘어난 스케일을 처음스케일로 변경
        }

    }
}