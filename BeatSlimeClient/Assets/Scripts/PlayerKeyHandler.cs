using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyHandler : MonoBehaviour
{
    public HexCell selfCoord;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            selfCoord.plus(-1, 0, 1);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            selfCoord.plus(0, -1, 1);
        else if(Input.GetKeyDown(KeyCode.E))
            selfCoord.plus(1, -1, 0);
        else if(Input.GetKeyDown(KeyCode.A))
            selfCoord.plus(-1, 1, 0);
        else if(Input.GetKeyDown(KeyCode.X))
            selfCoord.plus(0, 1, -1);
        else if(Input.GetKeyDown(KeyCode.D))
            selfCoord.plus(1, 0, -1);

    }
}
