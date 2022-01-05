using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMaker : MonoBehaviour
{
    public bool mapMakingOn;
    public HexCellPosition playerPosition;
    public HexGrid grid;
    public GameObject tmp;
    public int type = 0;
    public int dir = 0;
    public int w = 0;

    static public int cellId = 0;

    public List<Protocol.Map> Mapdata = new List<Protocol.Map>();

    // Update is called once per frame
    void Update()
    {
        if (mapMakingOn)
        {
            
            if (Input.GetKeyDown(KeyCode.U))
            {
                dir = 1;
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                dir = 2;
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                dir = 3;
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                dir = 4;
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                dir = 5;
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                dir = 6;
            }
            if (Input.GetKeyDown(KeyCode.LeftBracket))  //[
            {
                w--;
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))  //]
            {
                w++;
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))  //-
            {
                dir = 0;
            }
            if (Input.GetKeyDown(KeyCode.Minus))  //-
            {
                if (type >= 1)
                {
                    type--;
                    GameObject tmpcell = Instantiate(grid.cellType[type]);
                    tmpcell.name = "tmpTile";// + GameManager.data.mapCellid;
                    tmpcell.transform.parent = grid.transform;
                    tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X, playerPosition.coordinates.Z, w);

                    Destroy(tmp);
                    tmp = tmpcell;
                }
            }
            if (Input.GetKeyDown(KeyCode.Equals))  //=
            {
                if (type < grid.cellType.Count - 1)
                {
                    type++;

                    GameObject tmpcell = Instantiate(grid.cellType[type]);
                    tmpcell.name = "tmpTile";// + GameManager.data.mapCellid;
                    tmpcell.transform.parent = grid.transform;
                    tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X, playerPosition.coordinates.Z, w);

                    Destroy(tmp);
                    tmp = tmpcell;
                }
            }
            if (Input.GetKeyDown(KeyCode.Insert))  //Insert
            {
                switch(dir)
                {
                    case 1:
                        if (grid.cellMaps.map_Get(playerPosition.coordinates.X - 1, playerPosition.coordinates.Z + 1) == null)
                        {
                            GameObject tmpcell = Instantiate(grid.cellType[type]);
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X - 1, playerPosition.coordinates.Z + 1, w);
                            tmpcell.name = "cell";// + GameManager.data.mapCellid;
                            tmpcell.transform.parent = grid.transform;
                            grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X - 1, playerPosition.coordinates.Y, playerPosition.coordinates.Z + 1, w);

                            Protocol.Map p_tempcell = new Protocol.Map();
                            p_tempcell.type = 0;
                            p_tempcell.x = playerPosition.coordinates.X - 1;
                            p_tempcell.y = playerPosition.coordinates.Y;
                            p_tempcell.z = playerPosition.coordinates.Z + 1;
                            p_tempcell.w = w;
                            p_tempcell.color = type;
                            //p_tempcell.id = GameManager.data.mapCellid++;
                            p_tempcell.id = cellId++;

                            //GameManager.data.Mapdata.Add(p_tempcell);
                            Mapdata.Add(p_tempcell);
                            Debug.Log("@@@@@@@@@@@@@@@");
                        }
                        else
                        {
                            Debug.LogError(">MapTool : There are Cell exist already!<");
                        }
                        break;
                    case 2:
                        if (grid.cellMaps.map_Get(playerPosition.coordinates.X, playerPosition.coordinates.Z + 1) == null)
                        {
                            GameObject tmpcell = Instantiate(grid.cellType[type]);
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X, playerPosition.coordinates.Z + 1, w);
                            tmpcell.name = "cell";// + GameManager.data.mapCellid;
                            tmpcell.transform.parent = grid.transform;
                            grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X, playerPosition.coordinates.Y, playerPosition.coordinates.Z + 1, w);

                            Protocol.Map p_tempcell = new Protocol.Map();
                            p_tempcell.type = 0;
                            p_tempcell.x = playerPosition.coordinates.X;
                            p_tempcell.y = playerPosition.coordinates.Y;
                            p_tempcell.z = playerPosition.coordinates.Z + 1;
                            p_tempcell.w = w;
                            p_tempcell.color = type;
                            //p_tempcell.id = GameManager.data.mapCellid++;
                            p_tempcell.id = cellId++;

                            //GameManager.data.Mapdata.Add(p_tempcell);
                            Mapdata.Add(p_tempcell);
                            Debug.Log("@@@@@@@@@@@@@@@");
                        }
                        else
                        {
                            Debug.LogError(">MapTool : There are Cell exist already!<");
                        }
                        break;
                    case 3:
                        if (grid.cellMaps.map_Get(playerPosition.coordinates.X+1, playerPosition.coordinates.Z) == null)
                        {
                            GameObject tmpcell = Instantiate(grid.cellType[type]);
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X+1, playerPosition.coordinates.Z, w);
                            tmpcell.name = "cell";// + GameManager.data.mapCellid;
                            tmpcell.transform.parent = grid.transform;
                            grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X+1, playerPosition.coordinates.Y-1, playerPosition.coordinates.Z, w);

                            Protocol.Map p_tempcell = new Protocol.Map();
                            p_tempcell.type = 0;
                            p_tempcell.x = playerPosition.coordinates.X+1;
                            p_tempcell.y = playerPosition.coordinates.Y-1;
                            p_tempcell.z = playerPosition.coordinates.Z;
                            p_tempcell.w = w;
                            p_tempcell.color = type;
                            //p_tempcell.id = GameManager.data.mapCellid++;
                            p_tempcell.id = cellId++;

                            //GameManager.data.Mapdata.Add(p_tempcell);
                            Mapdata.Add(p_tempcell);
                            Debug.Log("@@@@@@@@@@@@@@@");
                        }
                        else
                        {
                            Debug.LogError(">MapTool : There are Cell exist already!<");
                        }
                        break;
                    case 4:
                        if (grid.cellMaps.map_Get(playerPosition.coordinates.X - 1, playerPosition.coordinates.Z) == null)
                        {
                            GameObject tmpcell = Instantiate(grid.cellType[type]);
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X - 1, playerPosition.coordinates.Z, w);
                            tmpcell.name = "cell";// + GameManager.data.mapCellid;
                            tmpcell.transform.parent = grid.transform;
                            grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X - 1, playerPosition.coordinates.Y+1, playerPosition.coordinates.Z, w);

                            Protocol.Map p_tempcell = new Protocol.Map();
                            p_tempcell.type = 0;
                            p_tempcell.x = playerPosition.coordinates.X - 1;
                            p_tempcell.y = playerPosition.coordinates.Y + 1;
                            p_tempcell.z = playerPosition.coordinates.Z;
                            p_tempcell.w = w;
                            p_tempcell.color = type;
                            //p_tempcell.id = GameManager.data.mapCellid++;
                            p_tempcell.id = cellId++;

                            //GameManager.data.Mapdata.Add(p_tempcell);
                            Mapdata.Add(p_tempcell);
                            Debug.Log("@@@@@@@@@@@@@@@");
                        }
                        else
                        {
                            Debug.LogError(">MapTool : There are Cell exist already!<");
                        }
                        break;
                    case 5:
                        if (grid.cellMaps.map_Get(playerPosition.coordinates.X, playerPosition.coordinates.Z-1) == null)
                        {
                            GameObject tmpcell = Instantiate(grid.cellType[type]);
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X, playerPosition.coordinates.Z-1, w);
                            tmpcell.name = "cell";// + GameManager.data.mapCellid;
                            tmpcell.transform.parent = grid.transform;
                            grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X, playerPosition.coordinates.Y+1, playerPosition.coordinates.Z-1, w);

                            Protocol.Map p_tempcell = new Protocol.Map();
                            p_tempcell.type = 0;
                            p_tempcell.x = playerPosition.coordinates.X;
                            p_tempcell.y = playerPosition.coordinates.Y+1;
                            p_tempcell.z = playerPosition.coordinates.Z-1;
                            p_tempcell.w = w;
                            p_tempcell.color = type;
                            //p_tempcell.id = GameManager.data.mapCellid++;
                            p_tempcell.id = cellId++;

                            //GameManager.data.Mapdata.Add(p_tempcell);
                            Mapdata.Add(p_tempcell);
                            Debug.Log("@@@@@@@@@@@@@@@");
                        }
                        else
                        {
                            Debug.LogError(">MapTool : There are Cell exist already!<");
                        }
                        break;
                    case 6:
                        if (grid.cellMaps.map_Get(playerPosition.coordinates.X + 1, playerPosition.coordinates.Z-1) == null)
                        {
                            GameObject tmpcell = Instantiate(grid.cellType[type]);
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X + 1, playerPosition.coordinates.Z-1, w);
                            tmpcell.name = "cell";// + GameManager.data.mapCellid;
                            tmpcell.transform.parent = grid.transform;
                            grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X + 1, playerPosition.coordinates.Y, playerPosition.coordinates.Z-1, w);

                            Protocol.Map p_tempcell = new Protocol.Map();
                            p_tempcell.type = 0;
                            p_tempcell.x = playerPosition.coordinates.X + 1;
                            p_tempcell.y = playerPosition.coordinates.Y;
                            p_tempcell.z = playerPosition.coordinates.Z - 1;
                            p_tempcell.w = w;
                            p_tempcell.color = type;
                            //p_tempcell.id = GameManager.data.mapCellid++;
                            p_tempcell.id = cellId++;

                            //GameManager.data.Mapdata.Add(p_tempcell);
                            Mapdata.Add(p_tempcell);
                            Debug.Log("@@@@@@@@@@@@@@@");
                        }
                        else
                        {
                            Debug.LogError(">MapTool : There are Cell exist already!<");
                        }
                        break;

                }
                dir = 0;
            }

            switch (dir)
            {
                case 0:
                    tmp.transform.position = new Vector3(1000f, 1000f, 1000f);
                    break;
                case 1:
                    tmp.transform.position = HexCellPosition.getRealPosition(playerPosition.coordinates.X-1, playerPosition.coordinates.Z+1, w);
                    break;
                case 2:
                    tmp.transform.position = HexCellPosition.getRealPosition(playerPosition.coordinates.X, playerPosition.coordinates.Z+1, w);
                    break;
                case 3:
                    tmp.transform.position = HexCellPosition.getRealPosition(playerPosition.coordinates.X+1, playerPosition.coordinates.Z, w);
                    break;
                case 4:
                    tmp.transform.position = HexCellPosition.getRealPosition(playerPosition.coordinates.X-1, playerPosition.coordinates.Z, w);
                    break;
                case 5:
                    tmp.transform.position = HexCellPosition.getRealPosition(playerPosition.coordinates.X, playerPosition.coordinates.Z-1, w);
                    break;
                case 6:
                    tmp.transform.position = HexCellPosition.getRealPosition(playerPosition.coordinates.X+1, playerPosition.coordinates.Z-1, w);
                    break;
                default:
                    tmp.transform.position = new Vector3(1000f, 1000f, 1000f);
                    break;
            }
        }
    }
}
