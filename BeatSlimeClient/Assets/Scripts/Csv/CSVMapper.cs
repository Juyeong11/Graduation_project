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

        for (int i=0; i<datas.Count; i++)
        {
            int x = int.Parse(datas[i]["x"].ToString());
            int y = int.Parse(datas[i]["y"].ToString());
            int z = int.Parse(datas[i]["z"].ToString());
            int w = int.Parse(datas[i]["w"].ToString());
            int c = int.Parse(datas[i]["color"].ToString());
            int t = int.Parse(datas[i]["type"].ToString());

            GameObject tmpcell = Instantiate(MM.grid.cellType[c]);
            tmpcell.GetComponent<HexCellPosition>().setInitPosition(x, z, w);
            tmpcell.name = "cell" + MapMaker.cellId++;
            tmpcell.transform.parent = gameObject.transform;
            MM.grid.cellMaps.Add(tmpcell, x, y, z, w);

            Protocol.Map p_tempcell = new Protocol.Map();
            p_tempcell.type = t;
            p_tempcell.x = x;
            p_tempcell.y = y;
            p_tempcell.z = z;
            p_tempcell.w = w;
            p_tempcell.color = c;

            MM.Mapdata.Add(p_tempcell);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.End))
        {
            using (var writer = new CsvFileWriter("Assets/Resources/MapCSV/" + mapName + ".csv"))
            {
                List<string> columns = new List<string>() { "id","x", "y", "z", "w", "color", "type"};// making Index Row
                writer.WriteRow(columns);
                columns.Clear();

                for (int i=0;i<MM.Mapdata.Count;++i)
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

            using (var writer = new CsvFileWriter("Assets/Resources/LandCSV/" + mapName + ".csv"))
            {
                List<string> columns = new List<string>() { "id", "x", "y", "z", "w","offX","offZ","offRotate", "color", "type" };// making Index Row
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
    }
}
