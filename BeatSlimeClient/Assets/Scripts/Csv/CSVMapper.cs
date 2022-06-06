using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
public class CSVMapper : MonoBehaviour
{
    public string mapName;
    public MapMaker MM;

    void Start()
    {

        List<Dictionary<string, object>> datas = CSVReader.Read("MapCSV/" + mapName);
        List<Dictionary<string, object>> landdatas = CSVReader.Read("LandCSV/" + mapName);

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

            //GameObject tmpcell = Instantiate(MM.grid.cellType[c]);
            GameObject tmpcell = Instantiate(MM.grid.cellMapOBJ.Cell[c]);
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
            p_templand.offScale = os;
            p_templand.color = c;
            p_templand.id = MapMaker.landId;

            //GameObject tmpcell = Instantiate(MM.grid.LandType[c]);
            GameObject tmpcell = Instantiate(MM.grid.cellMapOBJ.Land[c]);
            tmpcell.GetComponent<HexCellPosition>().landOffSetter(ox, oy, oz, or, os, t);
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
            PlayerPrefs.SetInt("MapMakerX", MM.playerPosition.coordinates.X);
            PlayerPrefs.SetInt("MapMakerW", MM.w);
            PlayerPrefs.SetInt("MapMakerZ", MM.playerPosition.coordinates.Z);
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

                //서버
                {

                    FileStream fs = new FileStream("Assets/Resources/MapCSV/" + mapName, FileMode.Create, FileAccess.Write);
                    BinaryWriter bw = new BinaryWriter(fs);


                    //좌표가 인덱스가 되어야함 -> 좌표에 마이너스값이 없어야함 -> 평행이동을 하자
                    //제일 앞에 얼마만큼 평행이동할지도 적어 두자 -> 가장 작은 값만큼 평행이동

                    int minX = System.Int32.MaxValue, minZ = System.Int32.MaxValue;
                    int maxX = System.Int32.MinValue, maxZ = System.Int32.MinValue;
                    for (int i = 0; i < MM.Mapdata.Count; ++i)
                    {
                        int x = MM.Mapdata[i].x;
                        int z = MM.Mapdata[i].z;

                        if (minX > x) minX = x;
                        if (minZ > z) minZ = z;

                        if (maxX < x) maxX = x;
                        if (maxZ < z) maxZ = z;
                    }
                    int LengthX = maxX - minX +4;
                    int LengthZ = maxZ - minZ +4;
                        Debug.Log("maxX : " + maxX + " maxZ : " + maxZ);
                        Debug.Log("minX : " + minX + " minZ : " + minZ);
                        Debug.Log(LengthZ* LengthX);

                    int[] data = Enumerable.Repeat<int>(-1, (LengthX)* LengthZ).ToArray<int>(); 
                    
                
                    for (int i = 0; i < MM.Mapdata.Count; ++i)
                    {
                        // x+y+z = 0
                        // x+z = -y
                        // x-min+z-min = -y + 2min

                        // 블록이 곂쳐저 있으면 덮어쓴다. 만약 블록 타입이 있는 타일이라면 덮어쓰지 말고 지나가자
                        int x = MM.Mapdata[i].x - minX;
                        int z = MM.Mapdata[i].z - minZ;
                        Debug.Log("x : " + x + "z : " + z);
                        Debug.Log("p : " + (x * LengthZ + z));
                        
                        if(data[z * LengthX + x] == 0 || data[z * LengthX + x] == -1)
                            data[z* LengthX + x] = MM.Mapdata[i].type;
                    }
                    // 평행이동 한 값, 총 개수..좌표들
                    //Debug.Log(min);

                    bw.Write(LengthX);
                    bw.Write(LengthZ);
                    bw.Write(minX);
                    bw.Write(minZ);
                    
                    for (int i = 0; i < LengthX * LengthZ; ++i)
                    {
                        bw.Write(data[i]);
                    }
                }

                Debug.Log(">Map \"" + mapName + "\" Saved!<");
            }
            else
            {
                using (var writer = new CsvFileWriter("Assets/Resources/LandCSV/" + mapName + ".csv"))
                {
                    List<string> columns = new List<string>() { "id", "x", "y", "z", "w", "offX", "offY", "offZ", "offRotate","offScale", "color", "type" };// making Index Row
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
                        columns.Add(MM.LandScapedata[i].offScale.ToString());
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
