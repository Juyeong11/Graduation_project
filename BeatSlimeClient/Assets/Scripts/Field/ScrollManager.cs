using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollManager : MonoBehaviour
{
    public GameObject scrollPrefab;
    public GameObject views;

    public void hasOpen()
    {
        gameObject.SetActive(true);
        for (int i = 0; i < 20; ++i)
        {
            if (PlayerPrefs.HasKey("inventory" + i))
            {
                if (PlayerPrefs.GetInt("inventory" + i) > 0)
                {
                    GameObject scroll = Instantiate(scrollPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    scroll.GetComponent<ScrollButons>().n.text = i.ToString();
                    scroll.GetComponent<ScrollButons>().s.text = PlayerPrefs.GetInt("inventory" + i).ToString();
                    scroll.GetComponent<ScrollButons>().init();
                    scroll.transform.SetParent(views.transform);
                    scroll.transform.localScale = new Vector3(1, 1, 1);
                    scroll.transform.localPosition = new Vector3(0, 0, 0);
                }
            }
        }

    }

    public void hasClose()
    {
        gameObject.SetActive(false);
        foreach(Transform i in views.transform)
        {
            Destroy(i.gameObject);
        }
    }

}
