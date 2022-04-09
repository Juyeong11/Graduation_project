using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BranchStartScene : MonoBehaviour
{
    bool isField;
    void Start()
    {
        isField = true;
        Invoke("Loaden",0.5f);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            isField = false;
        }
    }

    void Loaden()
    {
        if (isField)
        {
            SceneManager.LoadScene("FieldScene");
        }
        else
        {
            SceneManager.LoadScene("LoginScene");
        }
    }
}
