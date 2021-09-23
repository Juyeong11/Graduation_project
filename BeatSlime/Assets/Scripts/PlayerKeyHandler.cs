using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyHandler : MonoBehaviour
{
    void Update()
    {
        //절대 좌표가 아니라 hexagonal 좌표로 바꿔야 함

        if (Input.GetKeyDown(KeyCode.Q))
            transform.localPosition = new Vector3(transform.localPosition.x - 0.866f, transform.localPosition.y, transform.localPosition.z + 0.5f);
        else if (Input.GetKeyDown(KeyCode.W))
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + 1f);
        else if(Input.GetKeyDown(KeyCode.E))
            transform.localPosition = new Vector3(transform.localPosition.x + 0.866f, transform.localPosition.y, transform.localPosition.z + 0.5f);
        else if(Input.GetKeyDown(KeyCode.A))
            transform.localPosition = new Vector3(transform.localPosition.x - 0.866f, transform.localPosition.y, transform.localPosition.z - 0.5f);
        else if(Input.GetKeyDown(KeyCode.S))
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - 1f);
        else if(Input.GetKeyDown(KeyCode.D))
            transform.localPosition = new Vector3(transform.localPosition.x + 0.866f, transform.localPosition.y, transform.localPosition.z - 0.5f);

    }
}
