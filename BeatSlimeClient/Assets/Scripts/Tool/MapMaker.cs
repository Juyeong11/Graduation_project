using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMaker : MonoBehaviour
{
    public bool mapMakingOn;
    public HexCellPosition playerPosition;
    public HexGrid grid;

    public bool cellMode = true;

    public GameObject tmp;
    public GameObject tmpLand;
    public int type = 0;
    public int landType = 0;
    public int dir = 0;
    public int w = 0;

    public float landOffsetX = 0;
    public float landOffsetY = 0;
    public float landOffsetZ = 0;
    public int landOffsetRotate = 0;

    static public int cellId = 0;
    static public int landId = 0;

    public List<Protocol.Map> Mapdata = new List<Protocol.Map>();
    public List<Protocol.LandScape> LandScapedata = new List<Protocol.LandScape>();

    // Update is called once per frame
    void Update()
    {
        if (mapMakingOn)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                tmp.SetActive(true);
                tmpLand.SetActive(false);
                cellMode = true;
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                tmp.SetActive(false);
                tmpLand.SetActive(true);
                cellMode = false;
            }
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
            if (Input.GetKeyDown(KeyCode.N))
            {
                landOffsetRotate -= 10;
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                landOffsetRotate += 10;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                landOffsetX -= 0.05f;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                landOffsetX += 0.05f;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                landOffsetZ += 0.05f;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                landOffsetZ -= 0.05f;
            }
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                landOffsetY += 0.05f;
            }
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                landOffsetY -= 0.05f;
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
                if (cellMode)
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
                else
                {
                    if (landType >= 1)
                    {
                        landType--;
                        GameObject tmpland = Instantiate(grid.LandType[landType]);
                        tmpland.name = "tmpTile";// + GameManager.data.mapCellid;
                        tmpland.transform.parent = grid.transform;
                        tmpland.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X, playerPosition.coordinates.Z, w);

                        Destroy(tmpLand);
                        tmpLand = tmpland;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Equals))  //=
            {
                if (cellMode)
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
                else
                {
                    if (landType < grid.LandType.Count - 1)
                    {
                        landType++;
                        GameObject tmpland = Instantiate(grid.LandType[landType]);
                        tmpland.name = "tmpTile";// + GameManager.data.mapCellid;
                        tmpland.transform.parent = grid.transform;
                        tmpland.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X, playerPosition.coordinates.Z, w);

                        Destroy(tmpLand);
                        tmpLand = tmpland;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Insert))  //Insert
            {
                switch(dir)
                {
                    case 1:
                        if (cellMode)
                        {
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
                                Debug.Log("@@@@@@CELL@@@@@@");
                            }
                            else
                            {
                                Debug.LogError(">MapTool : There are Cell exist already!<");
                            }
                        }
                        else
                        {
                            GameObject tmpland = Instantiate(grid.LandType[landType]);
                            tmpland.GetComponent<HexCellPosition>().landOffSetter(landOffsetX, landOffsetY, landOffsetZ, landOffsetRotate);
                            tmpland.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X - 1, playerPosition.coordinates.Z + 1, w);
                            tmpland.name = "land";// + GameManager.data.mapCellid;
                            tmpland.transform.parent = grid.transform;
                            grid.cellMaps.Add(tmpland, playerPosition.coordinates.X - 1, playerPosition.coordinates.Y, playerPosition.coordinates.Z + 1, w);

                            Protocol.LandScape p_templand = new Protocol.LandScape();
                            p_templand.type = 0;
                            p_templand.x = playerPosition.coordinates.X - 1;
                            p_templand.y = playerPosition.coordinates.Y;
                            p_templand.z = playerPosition.coordinates.Z + 1;
                            p_templand.w = w;
                            p_templand.offX = landOffsetX;
                            p_templand.offY = landOffsetY;
                            p_templand.offZ = landOffsetZ;
                            p_templand.offRotate = landOffsetRotate;
                            p_templand.color = landType;
                            //p_tempcell.id = GameManager.data.mapCellid++;
                            p_templand.id = landId++;

                            //GameManager.data.Mapdata.Add(p_tempcell);
                            LandScapedata.Add(p_templand);
                            Debug.Log("@@@@@@LAND@@@@@@");
                        }
                        break;
                    case 2:
                        if (cellMode)
                        {
                            if (grid.cellMaps.map_Get(playerPosition.coordinates.X, playerPosition.coordinates.Z + 1) == null)
                            {
                                GameObject tmpcell = Instantiate(grid.cellType[type]);
                                tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X, playerPosition.coordinates.Z + 1, w);
                                tmpcell.name = "cell";// + GameManager.data.mapCellid;
                                tmpcell.transform.parent = grid.transform;
                                grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X, playerPosition.coordinates.Y - 1, playerPosition.coordinates.Z + 1, w);

                                Protocol.Map p_tempcell = new Protocol.Map();
                                p_tempcell.type = 0;
                                p_tempcell.x = playerPosition.coordinates.X;
                                p_tempcell.y = playerPosition.coordinates.Y - 1;
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
                        }
                        else
                        {
                            GameObject tmpcell = Instantiate(grid.LandType[landType]);
                            tmpcell.GetComponent<HexCellPosition>().landOffSetter(landOffsetX, landOffsetY, landOffsetZ, landOffsetRotate);
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X , playerPosition.coordinates.Z + 1, w);
                            tmpcell.name = "land";// + GameManager.data.mapCellid;
                            tmpcell.transform.parent = grid.transform;
                            grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X, playerPosition.coordinates.Y-1, playerPosition.coordinates.Z + 1, w);

                            Protocol.LandScape p_templand = new Protocol.LandScape();
                            p_templand.type = 0;
                            p_templand.x = playerPosition.coordinates.X;
                            p_templand.y = playerPosition.coordinates.Y - 1;
                            p_templand.z = playerPosition.coordinates.Z + 1;
                            p_templand.w = w;
                            p_templand.offX = landOffsetX;
                            p_templand.offY = landOffsetY;
                            p_templand.offZ = landOffsetZ;
                            p_templand.offRotate = landOffsetRotate;
                            p_templand.color = landType;
                            //p_tempcell.id = GameManager.data.mapCellid++;
                            p_templand.id = landId++;

                            //GameManager.data.Mapdata.Add(p_tempcell);
                            LandScapedata.Add(p_templand);
                            Debug.Log("@@@@@@LAND@@@@@@");
                        }
                        break;
                    case 3:
                        if (cellMode)
                        {
                            if (grid.cellMaps.map_Get(playerPosition.coordinates.X + 1, playerPosition.coordinates.Z) == null)
                            {
                                GameObject tmpcell = Instantiate(grid.cellType[type]);
                                tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X + 1, playerPosition.coordinates.Z, w);
                                tmpcell.name = "cell";// + GameManager.data.mapCellid;
                                tmpcell.transform.parent = grid.transform;
                                grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X + 1, playerPosition.coordinates.Y - 1, playerPosition.coordinates.Z, w);

                                Protocol.Map p_tempcell = new Protocol.Map();
                                p_tempcell.type = 0;
                                p_tempcell.x = playerPosition.coordinates.X + 1;
                                p_tempcell.y = playerPosition.coordinates.Y - 1;
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
                        }
                        else
                        {
                            GameObject tmpcell = Instantiate(grid.LandType[landType]);
                            tmpcell.GetComponent<HexCellPosition>().landOffSetter(landOffsetX, landOffsetY, landOffsetZ, landOffsetRotate);
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X + 1, playerPosition.coordinates.Z, w);
                            tmpcell.name = "land";// + GameManager.data.mapCellid;
                            tmpcell.transform.parent = grid.transform;
                            grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X + 1, playerPosition.coordinates.Y - 1, playerPosition.coordinates.Z, w);

                            Protocol.LandScape p_templand = new Protocol.LandScape();
                            p_templand.type = 0;
                            p_templand.x = playerPosition.coordinates.X + 1;
                            p_templand.y = playerPosition.coordinates.Y - 1;
                            p_templand.z = playerPosition.coordinates.Z;
                            p_templand.w = w;
                            p_templand.offX = landOffsetX;
                            p_templand.offY = landOffsetY;
                            p_templand.offZ = landOffsetZ;
                            p_templand.offRotate = landOffsetRotate;
                            p_templand.color = landType;
                            //p_tempcell.id = GameManager.data.mapCellid++;
                            p_templand.id = landId++;

                            //GameManager.data.Mapdata.Add(p_tempcell);
                            LandScapedata.Add(p_templand);
                            Debug.Log("@@@@@@LAND@@@@@@");
                        }
                        break;
                    case 4:
                        if (cellMode)
                        {
                            if (grid.cellMaps.map_Get(playerPosition.coordinates.X - 1, playerPosition.coordinates.Z) == null)
                            {
                                GameObject tmpcell = Instantiate(grid.cellType[type]);
                                tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X - 1, playerPosition.coordinates.Z, w);
                                tmpcell.name = "cell";// + GameManager.data.mapCellid;
                                tmpcell.transform.parent = grid.transform;
                                grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X - 1, playerPosition.coordinates.Y + 1, playerPosition.coordinates.Z, w);

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
                        }
                        else
                        {
                            GameObject tmpcell = Instantiate(grid.LandType[landType]);
                            tmpcell.GetComponent<HexCellPosition>().landOffSetter(landOffsetX, landOffsetY, landOffsetZ, landOffsetRotate);
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X - 1, playerPosition.coordinates.Z, w);
                            tmpcell.name = "land";// + GameManager.data.mapCellid;
                            tmpcell.transform.parent = grid.transform;
                            grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X - 1, playerPosition.coordinates.Y + 1, playerPosition.coordinates.Z, w);

                            Protocol.LandScape p_templand = new Protocol.LandScape();
                            p_templand.type = 0;
                            p_templand.x = playerPosition.coordinates.X - 1;
                            p_templand.y = playerPosition.coordinates.Y + 1;
                            p_templand.z = playerPosition.coordinates.Z;
                            p_templand.w = w;
                            p_templand.offX = landOffsetX;
                            p_templand.offY = landOffsetY;
                            p_templand.offZ = landOffsetZ;
                            p_templand.offRotate = landOffsetRotate;
                            p_templand.color = landType;
                            //p_tempcell.id = GameManager.data.mapCellid++;
                            p_templand.id = landId++;

                            //GameManager.data.Mapdata.Add(p_tempcell);
                            LandScapedata.Add(p_templand);
                            Debug.Log("@@@@@@LAND@@@@@@");
                        }
                        break;
                    case 5:
                        if (cellMode)
                        {
                            if (grid.cellMaps.map_Get(playerPosition.coordinates.X, playerPosition.coordinates.Z - 1) == null)
                            {
                                GameObject tmpcell = Instantiate(grid.cellType[type]);
                                tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X, playerPosition.coordinates.Z - 1, w);
                                tmpcell.name = "cell";// + GameManager.data.mapCellid;
                                tmpcell.transform.parent = grid.transform;
                                grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X, playerPosition.coordinates.Y + 1, playerPosition.coordinates.Z - 1, w);

                                Protocol.Map p_tempcell = new Protocol.Map();
                                p_tempcell.type = 0;
                                p_tempcell.x = playerPosition.coordinates.X;
                                p_tempcell.y = playerPosition.coordinates.Y + 1;
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
                        }
                        else
                        {
                            GameObject tmpcell = Instantiate(grid.LandType[landType]);
                            tmpcell.GetComponent<HexCellPosition>().landOffSetter(landOffsetX, landOffsetY, landOffsetZ, landOffsetRotate);
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X, playerPosition.coordinates.Z - 1, w);
                            tmpcell.name = "land";// + GameManager.data.mapCellid;
                            tmpcell.transform.parent = grid.transform;
                            grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X, playerPosition.coordinates.Y + 1, playerPosition.coordinates.Z - 1, w);

                            Protocol.LandScape p_templand = new Protocol.LandScape();
                            p_templand.type = 0;
                            p_templand.x = playerPosition.coordinates.X;
                            p_templand.y = playerPosition.coordinates.Y + 1;
                            p_templand.z = playerPosition.coordinates.Z - 1;
                            p_templand.w = w;
                            p_templand.offX = landOffsetX;
                            p_templand.offY = landOffsetY;
                            p_templand.offZ = landOffsetZ;
                            p_templand.offRotate = landOffsetRotate;
                            p_templand.color = landType;
                            //p_tempcell.id = GameManager.data.mapCellid++;
                            p_templand.id = landId++;

                            //GameManager.data.Mapdata.Add(p_tempcell);
                            LandScapedata.Add(p_templand);
                            Debug.Log("@@@@@@LAND@@@@@@");
                        }
                        break;
                    case 6:
                        if (cellMode)
                        {
                            if (grid.cellMaps.map_Get(playerPosition.coordinates.X + 1, playerPosition.coordinates.Z - 1) == null)
                            {
                                GameObject tmpcell = Instantiate(grid.cellType[type]);
                                tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X + 1, playerPosition.coordinates.Z - 1, w);
                                tmpcell.name = "cell";// + GameManager.data.mapCellid;
                                tmpcell.transform.parent = grid.transform;
                                grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X + 1, playerPosition.coordinates.Y, playerPosition.coordinates.Z - 1, w);

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
                        }
                        else
                        {
                            GameObject tmpcell = Instantiate(grid.LandType[landType]);
                            tmpcell.GetComponent<HexCellPosition>().landOffSetter(landOffsetX, landOffsetY, landOffsetZ, landOffsetRotate);
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X + 1, playerPosition.coordinates.Z - 1, w);
                            tmpcell.name = "land";// + GameManager.data.mapCellid;
                            tmpcell.transform.parent = grid.transform;
                            grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X + 1, playerPosition.coordinates.Y, playerPosition.coordinates.Z - 1, w);

                            Protocol.LandScape p_templand = new Protocol.LandScape();
                            p_templand.type = 0;
                            p_templand.x = playerPosition.coordinates.X + 1;
                            p_templand.y = playerPosition.coordinates.Y;
                            p_templand.z = playerPosition.coordinates.Z - 1;
                            p_templand.w = w;
                            p_templand.offX = landOffsetX;
                            p_templand.offY = landOffsetY;
                            p_templand.offZ = landOffsetZ;
                            p_templand.offRotate = landOffsetRotate;
                            p_templand.color = landType;
                            //p_tempcell.id = GameManager.data.mapCellid++;
                            p_templand.id = landId++;

                            //GameManager.data.Mapdata.Add(p_tempcell);
                            LandScapedata.Add(p_templand);
                            Debug.Log("@@@@@@LAND@@@@@@");
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
            tmpLand.transform.position = new Vector3(tmp.transform.position.x + landOffsetX, tmp.transform.position.y + landOffsetY, tmp.transform.position.z + landOffsetZ);
            tmpLand.transform.rotation = Quaternion.Euler(new Vector3(0, landOffsetRotate, 0));
        }
    }
}
