using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FieldHexGrid : MonoBehaviour
{
    public bool MakeMapWithoutMapPacket;

    private int xMaxLength;
    private int yMaxLength;
    private int zMaxLength;
    private int xMinLength;
    private int yMinLength;
    private int zMinLength;

    //Cell 종류
    public List<GameObject> cellType;
    public CellMap cellMaps;

    //위치
    public HexCoordinates pPosition;
    public HexCoordinates ePosition;

    public List<List<HexCoordinates>> RedZones;

    public FieldHexGrid()
    { 
        // LintJson 넣고 Json에서 읽어올 예정

        cellMaps = new CellMap();
        xMaxLength = 10;
        yMaxLength = 10;
        zMaxLength = 10;
        xMinLength = -10;
        yMinLength = -10;
        zMinLength = -10;
    }

    public void Beat()
    {
        if (cellMaps.Get(pPosition).state == cellState.Damaged)
        {
            if(!FieldGameManager.Net.isOnline)
            Debug.Log("Player Damaged!");
        }
        foreach(var cell in cellMaps.cellMaps)
        {
            cell.Beat();
        }
    }

    public override string ToString()
    {
        return "override this!";
    }
    public void Start()
    {
        int color = 0;
        RedZones = new List<List<HexCoordinates>>();

        if (MakeMapWithoutMapPacket)
        {
            //맵 생성
            for (int x = xMinLength; x <= xMaxLength; ++x)
            {
                for (int y = yMinLength; y <= yMaxLength; ++y)
                {
                    for (int z = zMinLength; z <= zMaxLength; ++z)
                    {
                        if (x + y + z == 0)
                        {
                            //print(cellType[0]);
                            GameObject tmpcell = Instantiate(cellType[color]); // <- 나중에 string name으로 바꿔야?
                            //int w = Random.Range(0, 3);
                            int w = 0;
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(x, z, w);
                            tmpcell.name = "cell" + FieldGameManager.data.mapCellid;
                            tmpcell.transform.parent = gameObject.transform;
                            cellMaps.Add(tmpcell, x, y, z, w);

                            Protocol.Map p_tempcell = new Protocol.Map();
                            p_tempcell.type = 0;
                            p_tempcell.x = x;
                            p_tempcell.y = y;
                            p_tempcell.z = z;
                            p_tempcell.w = w;
                            p_tempcell.color = 0;
                            p_tempcell.id = FieldGameManager.data.mapCellid++;

                            FieldGameManager.data.Mapdata.Add(p_tempcell);
                        }
                    }
                }
            }
        }
    }

    public void P_MakeHexMap(Protocol.Map map)
    {
        if (!MakeMapWithoutMapPacket)
        {
            if (map.x + map.y + map.z == 0)
            {
                //print(cellType[0]);
                GameObject tmpcell = Instantiate(cellType[map.color]); // <- 나중에 string name으로 바꿔야?
                tmpcell.GetComponent<HexCellPosition>().setInitPosition(map.x, map.z,map.w);
                tmpcell.name = "cell" + map.id;
                FieldGameManager.data.mapCellid = map.id+1;
                tmpcell.transform.parent = gameObject.transform;
                cellMaps.Add(tmpcell, map.x, map.y, map.z,map.w);
            }
            else
            {
                Debug.LogError(">>InValid HexCoordinate Error From MAP Packet<<");
            }
        }

        else
        {
            Debug.LogError(">>Flag_MakeMapWithoutMapPacket<<");
        }
    }

}
