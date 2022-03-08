using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    HexGrid grid;
    FieldHexGrid fGrid;
    public bool LoadByGameManagersSongName;
    public string MapName;
    public GameObject GRID;

    public void Match(HexGrid hg)
    {
        grid = hg;
    }

    public void Match(FieldHexGrid fhg)
    {
        fGrid = fhg;
    }

    public void LoadMap()
    {
        List<Dictionary<string, object>> datas;
        List<Dictionary<string, object>> landdatas;

        if (LoadByGameManagersSongName)
        {
            datas = CSVReader.Read("MapCSV/" + GameManager.data.SongName);
            landdatas = CSVReader.Read("LandCSV/" + GameManager.data.SongName);
        }
        else
        {
            datas = CSVReader.Read("MapCSV/" + MapName);
            landdatas = CSVReader.Read("LandCSV/" + MapName);
        }

        for (int i = 0; i < datas.Count; i++)
        {
            int x = int.Parse(datas[i]["x"].ToString());
            int y = int.Parse(datas[i]["y"].ToString());
            int z = int.Parse(datas[i]["z"].ToString());
            int w = int.Parse(datas[i]["w"].ToString());
            int c = int.Parse(datas[i]["color"].ToString());
            int t = int.Parse(datas[i]["type"].ToString());

            Protocol.Map p_tempcell = new Protocol.Map();
            p_tempcell.type = t;
            p_tempcell.x = x;
            p_tempcell.y = y;
            p_tempcell.z = z;
            p_tempcell.w = w;
            p_tempcell.color = c;
            p_tempcell.id = MapMaker.cellId;

            if (LoadByGameManagersSongName)
            {
                GameObject tmpcell = Instantiate(grid.cellType[c]);
                tmpcell.GetComponent<HexCellPosition>().setInitPosition(x, z, w);
                tmpcell.name = "cell" + MapMaker.cellId++;
                tmpcell.transform.parent = GRID.transform;
                grid.cellMaps.Add(tmpcell, x, y, z, w);
            }
            else
            {
                GameObject tmpcell = Instantiate(fGrid.cellType[c]);
                tmpcell.GetComponent<HexCellPosition>().setInitPosition(x, z, w);
                tmpcell.name = "cell" + MapMaker.cellId++;
                tmpcell.transform.parent = GRID.transform;
                fGrid.cellMaps.Add(tmpcell, x, y, z, w);
            }

            if(LoadByGameManagersSongName)
                GameManager.data.Mapdata.Add(p_tempcell);
            else
                FieldGameManager.data.Mapdata.Add(p_tempcell);
        }

        for (int i = 0; i < landdatas.Count; i++)
        {
            int x = int.Parse(landdatas[i]["x"].ToString());
            int y = int.Parse(landdatas[i]["y"].ToString());
            int z = int.Parse(landdatas[i]["z"].ToString());
            int w = int.Parse(landdatas[i]["w"].ToString());
            float ox = float.Parse(landdatas[i]["offX"].ToString());
            float oy = float.Parse(landdatas[i]["offY"].ToString());
            float oz = float.Parse(landdatas[i]["offZ"].ToString());
            float or = float.Parse(landdatas[i]["offRotate"].ToString());
            float os = float.Parse(landdatas[i]["offScale"].ToString());
            int c = int.Parse(landdatas[i]["color"].ToString());
            int t = int.Parse(landdatas[i]["type"].ToString());

            Protocol.LandScape p_templand = new Protocol.LandScape();
            p_templand.type = t;
            p_templand.x = x;
            p_templand.y = y;
            p_templand.z = z;
            p_templand.w = w;
            p_templand.offX = ox;
            p_templand.offY = oy;
            p_templand.offZ = oz;
            p_templand.offRotate = or;
            p_templand.color = c;
            p_templand.id = MapMaker.landId;

            GameObject tmpcell = Instantiate(LoadByGameManagersSongName?GameManager.data.grid.LandType[c]:FieldGameManager.data.grid.LandType[c]);
            tmpcell.GetComponent<HexCellPosition>().landOffSetter(ox, oy, oz, or, os);
            tmpcell.GetComponent<HexCellPosition>().setInitPosition(x, z, w);
            tmpcell.name = "land" + MapMaker.landId++;
            tmpcell.transform.parent = GRID.transform;
            //MM.grid.cellMaps.Add(tmpcell, x, y, z, w);
        }
    }
}
