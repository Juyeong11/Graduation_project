using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ST_move : MonoBehaviour
{
    Vector3 PlayerPos;


    const float Displacement = 1 - 0.01f;

    const float MaxPos = 3.5f * Displacement;
    const float MinPos = -3.5f * Displacement;
    // Start is called before the first frame update
    void Start()
    {
        PlayerPos = new Vector3(MinPos, MaxPos, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.W))
        {
            PlayerPos.y += Displacement;
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            PlayerPos.y -= Displacement;

        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            PlayerPos.x -= Displacement;

        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            PlayerPos.x += Displacement;

        }
        PlayerPos.x = Mathf.Clamp(PlayerPos.x, MinPos, MaxPos);
        PlayerPos.y = Mathf.Clamp(PlayerPos.y, MinPos, MaxPos);
        gameObject.transform.position = PlayerPos;
    }
}

