using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSVMapper : MonoBehaviour
{
    public string mapName;
    public MapMaker MM;

    void Start()
    {
        List<Dictionary<string,object>> datas = CSVReader.Read("MapCSV/" + mapName);
        List<Dictionary<string, object>> landdatas = CSVReader.Read("LandCSV/" + mapName);

        for (int i=0; i<datas.Count; i++)
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

            GameObject tmpcell = Instantiate(MM.grid.cellType[c]);
            tmpcell.GetComponent<HexCellPosition>().setInitPosition(x, z, w);
            tmpcell.name = "cell" + MapMaker.cellId++;
            tmpcell.transform.parent = gameObject.transform;
            MM.grid.cellMaps.Add(tmpcell, x, y, z, w);

            MM.Mapdata.Add(p_tempcell);
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

            GameObject tmpcell = Instantiate(MM.grid.LandType[c]);
            tmpcell.GetComponent<HexCellPosition>().landOffSetter(ox,oy,oz,or);
            tmpcell.GetComponent<HexCellPosition>().setInitPosition(x, z, w);
            tmpcell.name = "land" + MapMaker.landId++;
            tmpcell.transform.parent = gameObject.transform;
            //MM.grid.cellMaps.Add(tmpcell, x, y, z, w);

            MM.LandScapedata.Add(p_templand);
        }

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.End))
        {
            if (MM.cellMode)
            {
                using (var writer = new CsvFileWriter("Assets/Resources/MapCSV/" + mapName + ".csv"))
                {
                    List<string> columns = new List<string>() { "id", "x", "y", "z", "w", "color", "type" };// making Index Row
                    writer.WriteRow(columns);
                    columns.Clear();

                    for (int i = 0; i < MM.Mapdata.Count; ++i)
                    {
                        columns.Add(MM.Mapdata[i].id.ToString());
                        columns.Add(MM.Mapdata[i].x.ToString());
                        columns.Add(MM.Mapdata[i].y.ToString());
                        columns.Add(MM.Mapdata[i].z.ToString());
                        columns.Add(MM.Mapdata[i].w.ToString());
                        columns.Add(MM.Mapdata[i].color.ToString());
                        columns.Add(MM.Mapdata[i].type.ToString());
                        writer.WriteRow(columns);
                        columns.Clear();
                    }
                }

                Debug.Log(">Map \"" + mapName + "\" Saved!<");
            }
            else
            {
                using (var writer = new CsvFileWriter("Assets/Resources/LandCSV/" + mapName + ".csv"))
                {
                    List<string> columns = new List<string>() { "id", "x", "y", "z", "w", "offX", "offY", "offZ", "offRotate", "color", "type" };// making Index Row
                    writer.WriteRow(columns);
                    columns.Clear();

                    for (int i = 0; i < MM.LandScapedata.Count; ++i)
                    {
                        columns.Add(MM.LandScapedata[i].id.ToString());
                        columns.Add(MM.LandScapedata[i].x.ToString());
                        columns.Add(MM.LandScapedata[i].y.ToString());
                        columns.Add(MM.LandScapedata[i].z.ToString());
                        columns.Add(MM.LandScapedata[i].w.ToString());
                        columns.Add(MM.LandScapedata[i].offX.ToString());
                        columns.Add(MM.LandScapedata[i].offY.ToString());
                        columns.Add(MM.LandScapedata[i].offZ.ToString());
                        columns.Add(MM.LandScapedata[i].offRotate.ToString());
                        columns.Add(MM.LandScapedata[i].color.ToString());
                        columns.Add(MM.LandScapedata[i].type.ToString());
                        writer.WriteRow(columns);
                        columns.Clear();
                    }
                }

                Debug.Log(">Land \"" + mapName + "\" Saved!<");
            }

        }
    }
}
