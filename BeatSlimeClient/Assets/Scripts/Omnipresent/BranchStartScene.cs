using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BranchStartScene : MonoBehaviour
{
    bool isField;
    void Start()
    {
        SceneManager.LoadScene("FieldScene");
        isField = true;
        //Invoke("Loaden",3f);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            SceneManager.LoadScene("LoginScene");
        }
        if (Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("FieldScene");
        }
    }

    void Loaden()
    {
        if (isField)
        {
            SceneManager.LoadScene("FieldScene");
        }
    }
}
