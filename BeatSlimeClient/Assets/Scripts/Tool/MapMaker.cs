using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public float landOffsetScale = 0;

    public SANDBOXSoundTestProj soundscape;

    static public int cellId = 0;
    static public int landId = 0;

    float purpler = 0f;

    public Text witch;

    public GameObject recipt;

    public List<Protocol.Map> Mapdata = new List<Protocol.Map>();
    public List<Protocol.LandScape> LandScapedata = new List<Protocol.LandScape>();

    void Start()
    {
        w = PlayerPrefs.GetInt("MapMakerW", 0);
    }
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
            if (Input.GetKeyDown(KeyCode.F1))
            {
                recipt.SetActive(true);
            }
            if (Input.GetKeyUp(KeyCode.F1))
            {
                recipt.SetActive(false);
            }
            if (Input.GetKey(KeyCode.F2))
            {
                purpler += Time.deltaTime;
            }
            if (Input.GetKeyUp(KeyCode.F2))
            {
                soundscape.SetSound(purpler);
                purpler = 0f;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("LoginScene");
            }
            if (Input.GetKeyDown(KeyCode.F4))
            {
                grid.cellMaps.map_Get(playerPosition.coordinates.X, playerPosition.coordinates.Z);
                {
                    foreach(var v in Mapdata)
                    {
                        if (v.x == playerPosition.coordinates.X &&  v.z == playerPosition.coordinates.Z)
                        {
                            v.type = 10;
                            Debug.Log("SHOP");
                            break;
                        }
                    }
                }
                
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
                landOffsetY += 0.02f;
            }
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                landOffsetY -= 0.02f;
            }
            if (Input.GetKeyDown(KeyCode.LeftBracket))  //[
            {
                w--;
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))  //]
            {
                w++;
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                PlayerPrefs.SetInt("MapMakerX", playerPosition.coordinates.X);
                PlayerPrefs.SetInt("MapMakerW", w);
                PlayerPrefs.SetInt("MapMakerZ", playerPosition.coordinates.Z);
                int scene = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(scene, LoadSceneMode.Single);
                Time.timeScale = 1;
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))  //-
            {
                dir = 0;
                landOffsetX = 0f;
                landOffsetY = 0f;
                landOffsetZ = 0f;
                landOffsetRotate = 0;
                tmpLand.transform.localScale = new Vector3(tmpLand.transform.localScale.x - landOffsetScale, tmpLand.transform.localScale.y - landOffsetScale, tmpLand.transform.localScale.z - landOffsetScale);
                landOffsetScale = 0f;
                w = 0;
            }
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                landOffsetScale -= 0.1f;
                tmpLand.transform.localScale = new Vector3(tmpLand.transform.localScale.x - 0.1f, tmpLand.transform.localScale.y - 0.1f, tmpLand.transform.localScale.z - 0.1f);
            }
            if (Input.GetKeyDown(KeyCode.Period))
            {
                landOffsetScale += 0.1f;
                tmpLand.transform.localScale = new Vector3(tmpLand.transform.localScale.x + 0.1f, tmpLand.transform.localScale.y + 0.1f, tmpLand.transform.localScale.z + 0.1f);
            }
            if (Input.GetKeyDown(KeyCode.Minus))  //-
            {
                if (cellMode)
                {
                    if (type >= 1)
                    {
                        type--;
                        //GameObject tmpcell = Instantiate(grid.cellType[type]);
                        GameObject tmpcell = Instantiate(grid.cellMapOBJ.Cell[type]);
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
                        //GameObject tmpland = Instantiate(grid.LandType[landType]);
                        GameObject tmpland = Instantiate(grid.cellMapOBJ.Land[landType]);
                        
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
                    //if (type < grid.cellType.Count - 1)
                    if (type < grid.cellMapOBJ.Cell.Count - 1)
                    {
                        type++;

                        //GameObject tmpcell = Instantiate(grid.cellType[type]);
                        GameObject tmpcell = Instantiate(grid.cellMapOBJ.Cell[type]);
                        tmpcell.name = "tmpTile";// + GameManager.data.mapCellid;
                        tmpcell.transform.parent = grid.transform;
                        tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X, playerPosition.coordinates.Z, w);

                        Destroy(tmp);
                        tmp = tmpcell;
                    }
                }
                else
                {
                    if (landType < grid.cellMapOBJ.Land.Count - 1)
                    {
                        landType++;
                        //GameObject tmpland = Instantiate(grid.LandType[landType]);
                        GameObject tmpland = Instantiate(grid.cellMapOBJ.Land[landType]);
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
                switch (dir)
                {
                    case 1:
                        if (cellMode)
                        {
                            if (grid.cellMaps.map_Get(playerPosition.coordinates.X - 1, playerPosition.coordinates.Z + 1) == null)
                            {
                                //GameObject tmpcell = Instantiate(grid.cellType[type]);
                                GameObject tmpcell = Instantiate(grid.cellMapOBJ.Cell[type]);
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
                            //GameObject tmpland = Instantiate(grid.LandType[landType]);
                            GameObject tmpland = Instantiate(grid.cellMapOBJ.Land[landType]);
                            tmpland.GetComponent<HexCellPosition>().landOffSetter(landOffsetX, landOffsetY, landOffsetZ, landOffsetRotate,landOffsetScale, 0);
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
                            p_templand.offScale = landOffsetScale;
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
                                //GameObject tmpcell = Instantiate(grid.cellType[type]);
                                GameObject tmpcell = Instantiate(grid.cellMapOBJ.Cell[type]);
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
                            //GameObject tmpcell = Instantiate(grid.LandType[landType]);
                            GameObject tmpcell = Instantiate(grid.cellMapOBJ.Land[landType]);
                            tmpcell.GetComponent<HexCellPosition>().landOffSetter(landOffsetX, landOffsetY, landOffsetZ, landOffsetRotate, landOffsetScale,0);
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(playerPosition.coordinates.X, playerPosition.coordinates.Z + 1, w);
                            tmpcell.name = "land";// + GameManager.data.mapCellid;
                            tmpcell.transform.parent = grid.transform;
                            grid.cellMaps.Add(tmpcell, playerPosition.coordinates.X, playerPosition.coordinates.Y - 1, playerPosition.coordinates.Z + 1, w);

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
                            p_templand.offScale = landOffsetScale;
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
                                //GameObject tmpcell = Instantiate(grid.cellType[type]);
                                GameObject tmpcell = Instantiate(grid.cellMapOBJ.Cell[type]);
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
                            //GameObject tmpcell = Instantiate(grid.LandType[landType]);
                            GameObject tmpcell = Instantiate(grid.cellMapOBJ.Land[landType]);
                            tmpcell.GetComponent<HexCellPosition>().landOffSetter(landOffsetX, landOffsetY, landOffsetZ, landOffsetRotate, landOffsetScale,0);
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
                            p_templand.offScale = landOffsetScale;
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
                                //GameObject tmpcell = Instantiate(grid.cellType[type]);
                                GameObject tmpcell = Instantiate(grid.cellMapOBJ.Cell[type]);
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
                            //GameObject tmpcell = Instantiate(grid.LandType[landType]);
                            GameObject tmpcell = Instantiate(grid.cellMapOBJ.Land[landType]);
                            tmpcell.GetComponent<HexCellPosition>().landOffSetter(landOffsetX, landOffsetY, landOffsetZ, landOffsetRotate, landOffsetScale,0);
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
                            p_templand.offScale = landOffsetScale;
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
                                //GameObject tmpcell = Instantiate(grid.cellType[type]);
                                GameObject tmpcell = Instantiate(grid.cellMapOBJ.Cell[type]);
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
                            //GameObject tmpcell = Instantiate(grid.LandType[landType]);
                            GameObject tmpcell = Instantiate(grid.cellMapOBJ.Land[landType]);
                            tmpcell.GetComponent<HexCellPosition>().landOffSetter(landOffsetX, landOffsetY, landOffsetZ, landOffsetRotate, landOffsetScale,0);
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
                            p_templand.offScale = landOffsetScale;
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
                                //GameObject tmpcell = Instantiate(grid.cellType[type]);
                                GameObject tmpcell = Instantiate(grid.cellMapOBJ.Cell[type]);
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
                            //GameObject tmpcell = Instantiate(grid.LandType[landType]);
                            GameObject tmpcell = Instantiate(grid.cellMapOBJ.Land[landType]);
                            tmpcell.GetComponent<HexCellPosition>().landOffSetter(landOffsetX, landOffsetY, landOffsetZ, landOffsetRotate, landOffsetScale,0);
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
                            p_templand.offScale = landOffsetScale;
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
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (cellMode)
                {
                    if (grid.cellMaps.map_Get(playerPosition.coordinates.X, playerPosition.coordinates.Z) != null)
                    {
                        if (grid.cellMaps.Remove(playerPosition.coordinates.X, playerPosition.coordinates.Z))
                        {
                            string id = Mapdata.Find(cell => cell.x == playerPosition.coordinates.X && cell.z == playerPosition.coordinates.Z).id.ToString();
                            Mapdata.RemoveAll(cell => cell.x == playerPosition.coordinates.X && cell.z == playerPosition.coordinates.Z);
                            GameObject.Find("cell" + id).SetActive(false);
                            Debug.Log("@@@@@@DELETE@@@@@@");
                        }
                        else
                        {
                            Debug.LogError("@@@@@@cell delete error@@@@@@");
                        }
                        //Mapdata.Add();

                    }
                }
                else
                {
                    int index = -1, finder = 0;
                    string id = "-1";
                    //맨 마지막 장식만 삭제
                    foreach (var land in LandScapedata)
                    {
                        if (land.x == playerPosition.coordinates.X && land.z == playerPosition.coordinates.Z)
                        {
                            id = land.id.ToString();
                            index = finder;
                        }
                        finder++;
                    }
                    if (index >= 0)
                    {
                        LandScapedata.RemoveAt(index);
                        GameObject.Find("land" + id).SetActive(false);
                        Debug.Log("@@@@@@LAND DELETE@@@@@@");
                    }
                }

                {
                    //switch (dir)
                    //{
                    //    case 1:
                    //        if (cellMode)
                    //        {
                    //            if (grid.cellMaps.map_Get(playerPosition.coordinates.X - 1, playerPosition.coordinates.Z + 1) != null)
                    //            {
                    //                Protocol.Map tmp = null;
                    //                foreach(var cell in Mapdata)
                    //                {
                    //                    if (cell.x == playerPosition.coordinates.X - 1 && cell.z == playerPosition.coordinates.Z)
                    //                    {
                    //                        tmp = cell;
                    //                    }
                    //                }
                    //                if (grid.cellMaps.Remove(playerPosition.coordinates.X - 1, playerPosition.coordinates.Z + 1))
                    //                {
                    //                    Mapdata.Remove(tmp);
                    //
                    //                    Debug.Log("@@@@@@DELETE@@@@@@");
                    //                }
                    //                else
                    //                {
                    //                    Debug.LogError("@@@@@@cell delete error@@@@@@");
                    //                }
                    //                //Mapdata.Add();
                    //
                    //            }
                    //        }
                    //        else
                    //        {  
                    //            //LandScapedata.Add(p_templand);
                    //            Debug.Log("@@@@@@LAND@@@@@@");
                    //        }
                    //        break;
                    //    case 2:
                    //        if (cellMode)
                    //        {
                    //            if (grid.cellMaps.map_Get(playerPosition.coordinates.X, playerPosition.coordinates.Z + 1) != null)
                    //            {
                    //                //Mapdata.Add(p_tempcell);
                    //                Debug.Log("@@@@@@@@@@@@@@@");
                    //            }
                    //
                    //        }
                    //        else
                    //        {
                    //            //LandScapedata.Add(p_templand);
                    //            Debug.Log("@@@@@@LAND@@@@@@");
                    //        }
                    //        break;
                    //    case 3:
                    //        if (cellMode)
                    //        {
                    //            if (grid.cellMaps.map_Get(playerPosition.coordinates.X + 1, playerPosition.coordinates.Z) != null)
                    //            {
                    //                //Mapdata.Add(p_tempcell);
                    //                Debug.Log("@@@@@@@@@@@@@@@");
                    //            }
                    //        }
                    //        else
                    //        {
                    //            //LandScapedata.Add(p_templand);
                    //            Debug.Log("@@@@@@LAND@@@@@@");
                    //        }
                    //        break;
                    //    case 4:
                    //        if (cellMode)
                    //        {
                    //            if (grid.cellMaps.map_Get(playerPosition.coordinates.X - 1, playerPosition.coordinates.Z) != null)
                    //            {
                    //                //Mapdata.Add(p_tempcell);
                    //                Debug.Log("@@@@@@@@@@@@@@@");
                    //            }
                    //        }
                    //        else
                    //        {
                    //            //LandScapedata.Add(p_templand);
                    //            Debug.Log("@@@@@@LAND@@@@@@");
                    //        }
                    //        break;
                    //    case 5:
                    //        if (cellMode)
                    //        {
                    //            if (grid.cellMaps.map_Get(playerPosition.coordinates.X, playerPosition.coordinates.Z - 1) != null)
                    //            {
                    //                //Mapdata.Add(p_tempcell);
                    //                Debug.Log("@@@@@@@@@@@@@@@");
                    //            }
                    //        }
                    //        else
                    //        {
                    //            //LandScapedata.Add(p_templand);
                    //            Debug.Log("@@@@@@LAND@@@@@@");
                    //        }
                    //        break;
                    //    case 6:
                    //        if (cellMode)
                    //        {
                    //            if (grid.cellMaps.map_Get(playerPosition.coordinates.X + 1, playerPosition.coordinates.Z - 1) != null)
                    //            {
                    //                //Mapdata.Add(p_tempcell);
                    //                Debug.Log("@@@@@@@@@@@@@@@");
                    //            }
                    //        }
                    //        else
                    //        {
                    //            //LandScapedata.Add(p_templand);
                    //            Debug.Log("@@@@@@LAND@@@@@@");
                    //        }
                    //        break;
                    //
                    //}
                }
                dir = 0;
            }

            HexCoordinates t = new HexCoordinates();
            t = playerPosition.coordinates;
            switch (dir)
            {
                case 0:
                    t.X = 1000;
                    t.Z = 1000;
                    break;
                case 1:
                    t.X -= 1;
                    t.Z += 1;
                    break;
                case 2:
                    t.Z += 1;
                    break;
                case 3:
                    t.X += 1;
                    break;
                case 4:
                    t.X -= 1;
                    break;
                case 5:
                    t.Z -= 1;
                    break;
                case 6:
                    t.X += 1;
                    t.Z -= 1;
                    break;
                default:
                    t.X = 1000;
                    t.Z = 1000;
                    break;
            }
            tmp.transform.position = HexCellPosition.getRealPosition(t.X,t.Z,w);

            tmpLand.transform.position = new Vector3(tmp.transform.position.x + landOffsetX, tmp.transform.position.y + landOffsetY, tmp.transform.position.z + landOffsetZ);
            tmpLand.transform.rotation = Quaternion.Euler(new Vector3(0, landOffsetRotate, 0));

            witch.text = "( " + (t.X == 1000 ? playerPosition.coordinates.X : t.X) + ", " + (t.Y == -2000 ? playerPosition.coordinates.Y : t.Y) + ", " + (t.Z == 1000 ? playerPosition.coordinates.Z : t.Z) + " ) , " + w + "\n"
                + (cellMode ? type : landType) + ", offset-> " + landOffsetX + ", " + landOffsetY + ", " + landOffsetZ + ", rotate : " + landOffsetRotate + ", Scale : " + landOffsetScale;

}
    }
}
