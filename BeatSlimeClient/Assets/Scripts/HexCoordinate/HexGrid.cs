using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HexGrid : MonoBehaviour
{
    public string loadFile;

    private int xMaxLength;
    private int yMaxLength;
    private int zMaxLength;
    private int xMinLength;
    private int yMinLength;
    private int zMinLength;

    public List<GameObject> cellType;
    public List<GameObject> cellMaps;

    public HexGrid()
    {
        // LintJson 넣고 Json에서 읽어올 예정

        cellMaps = new List<GameObject>();
        xMaxLength = 3;
        yMaxLength = 3;
        zMaxLength = 3;
        xMinLength = -3;
        yMinLength = -3;
        zMinLength = -3;
    }

    public override string ToString()
    {
        return "override this!";
    }
    public void Start()
    {
        for (int x = xMinLength; x <= xMaxLength; ++x)
        {
            for (int y = yMinLength; y <= yMaxLength; ++y)
            {
                for (int z = zMinLength; z <= zMaxLength; ++z)
                {
                    if (x + y + z == 0)
                    {
                        print(cellType[0]);
                        GameObject tmpcell = Instantiate(cellType[0]);
                        tmpcell.GetComponent<HexCell>().setInitPosition(x, z);
                        tmpcell.name = "cell"+x+y+z;
                        tmpcell.transform.parent = gameObject.transform;
                        cellMaps.Add(tmpcell);
                    }
                }
            }
        }

    }

}
