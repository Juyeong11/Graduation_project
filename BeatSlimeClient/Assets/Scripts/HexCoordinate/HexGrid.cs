using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum cellState
{
    Normal,
    Damaged,
    None,
    Shop,

    Orgel,

    Stage1Portal,
    Stage1_2Portal,
    Stage2Portal,
    Stage2_2Portal,

    Item1,
    Item2,
    Item3,
    Item4,
    Item5,
    Item6,
}

public class Cell
{
    public GameObject obejct;
    public int x;
    public int y;
    public int z;
    public int w;

    public int duration = 0;
    public cellState state
    {
        get;
        set;
    }

    public Cell()
    {
        obejct = null;
        x = 1000;
        y = 1000;
        z = 1000;
        w = 1000;
        state = cellState.None;
    }

    public Cell(GameObject cell, int x, int y,int z, int w, cellState state)
    {
        obejct = cell;
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
        this.state = state;
    }

    public (int,int,int, int) getCoordinate()
    {
        return (x, y, z, w);
    }

    public Vector3 getCellRealPosition()
    {
        if (obejct == null) return new Vector3(0, -1, 0);
        Vector3 neu = new Vector3();
        neu.x = obejct.transform.position.x;
        neu.y = obejct.transform.position.y + 0.128f;   ///
        neu.z = obejct.transform.position.z;

        return neu;
    }

    public void Warning()
    {
        
    }

    public void Damage(int d)
    {
        duration = d;
        state = cellState.Damaged;
    }

    public void Beat()
    {
        if (duration <= 0)
        {
            if (state == cellState.Damaged)
            {
                state = cellState.Normal;
            }
        }
        else
        {
            duration--;
        }
    }

    public void SetW(int wPos)
    {

    }

    public void GetItemFromThisCell()
    {
        state = cellState.Normal;
        obejct.transform.Find("#VFX_item").gameObject.SetActive(false);
        // Item 끄기
    }
    public void SetItemToThisCell(cellState item)
    {
        state = item;
        obejct.transform.Find("#VFX_item").gameObject.SetActive(true);
        // Item 켜기
    }
}

public class CellMap
{
    public List<Cell> cellMaps = new List<Cell>();

    public void Add(GameObject cell,int x, int y,int z,int w=0)
    {
        cellMaps.Add(new Cell(cell, x, y, z,w, cellState.Normal));
    }
    public void Add(GameObject cell, int x, int y, int z, int w, cellState state)
    {
        cellMaps.Add(new Cell(cell, x, y, z,w, state));
    }

    public bool Remove(int x, int z)
    {
        int cells = cellMaps.Count;
        cellMaps.RemoveAll(cell => cell.x == x && cell.z == z);

        if (cells == cellMaps.Count)
            return false;

        return true;
    }
    public bool SetW(int x, int z,float offset)
    {
        foreach (var v in cellMaps)
        {
            if (v.getCoordinate().Item1 == x &&
                v.getCoordinate().Item3 == z)
            {
                v.obejct.GetComponent<HexCellPosition>().setHeightoffset(offset);
                
                return true;
            }
        }

        //Debug.LogError(x + ">Invalid coordinate<" + z);

        return false;
    }
    public bool plusW(int x, int z, float offset)
    {
        foreach (var v in cellMaps)
        {
            if (v.getCoordinate().Item1 == x &&
                v.getCoordinate().Item3 == z)
            {
                v.obejct.GetComponent<HexCellPosition>().plusHeightoffset(offset);

                return true;
            }
        }

        //Debug.LogError(x + ">Invalid coordinate<" + z);

        return false;
    }
    public Cell Get(int x,int y,int z)
    {
        foreach(var v in cellMaps)
        {
            if (v.getCoordinate().Item1 == x &&
                v.getCoordinate().Item2 == y &&
                v.getCoordinate().Item3 == z )
            {
                return v;
            }
        }

        Debug.LogError(">Invalid coordinate<");
        return new Cell();
    }

    public Cell map_Get(int x, int z)
    {
        foreach (var v in cellMaps)
        {
            if (v.getCoordinate().Item1 == x &&
                v.getCoordinate().Item2 == -(x+z) &&
                v.getCoordinate().Item3 == z)
            {
                return v;
            }
        }

        //Debug.LogError(">MapTool<");
        return null;
    }

    public Cell Get(HexCoordinates H)
    {
        foreach (var v in cellMaps)
        {
            if (v.getCoordinate().Item1 == H.X &&
                v.getCoordinate().Item2 == H.Y &&
                v.getCoordinate().Item3 == H.Z)
            {
                return v;
            }
        }

        Debug.LogError(">Invalid coordinate<");
        return null;
    }

}

[System.Serializable]
public class HexGrid : MonoBehaviour
{
    public bool MakeMapWithoutMapPacket;
    public bool TMP;

    private int xMaxLength;
    private int yMaxLength;
    private int zMaxLength;
    private int xMinLength;
    private int yMinLength;
    private int zMinLength;

    //Cell 종류
    public CellMapOBJ cellMapOBJ;

    //public List<GameObject> cellType;
    //장식 종류
    //public List<GameObject> LandType;

    public CellMap cellMaps;

    //위치
    public HexCoordinates pPosition;
    public HexCoordinates ePosition;

    public List<List<HexCoordinates>> RedZones;

    public HexGrid()
    { 
        // LintJson 넣고 Json에서 읽어올 예정

        cellMaps = new CellMap();
        xMaxLength = 3;
        yMaxLength = 3;
        zMaxLength = 3;
        xMinLength = -3;
        yMinLength = -3;
        zMinLength = -3;
    }

    public void Beat()
    {
        if (cellMaps.Get(pPosition).state == cellState.Damaged)
        {
            if(!Network.isOnline)
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

        if (MakeMapWithoutMapPacket && !TMP)
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
                            //GameObject tmpcell = Instantiate(cellType[color]); // <- 나중에 string name으로 바꿔야?
                            GameObject tmpcell = Instantiate(cellMapOBJ.Cell[color]);
                            //int w = Random.Range(0, 3);
                            int w = 0;
                            tmpcell.GetComponent<HexCellPosition>().setInitPosition(x, z, w);
                            if (GameManager.data)
                                tmpcell.name = "cell" + GameManager.data.mapCellid;
                            tmpcell.transform.parent = gameObject.transform;
                            cellMaps.Add(tmpcell, x, y, z, w);

                            Protocol.Map p_tempcell = new Protocol.Map();
                            p_tempcell.type = 0;
                            p_tempcell.x = x;
                            p_tempcell.y = y;
                            p_tempcell.z = z;
                            p_tempcell.w = w;
                            p_tempcell.color = 0;
                            if (GameManager.data)
                                p_tempcell.id = GameManager.data.mapCellid++;

                            if (GameManager.data)
                                GameManager.data.Mapdata.Add(p_tempcell);
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
                //GameObject tmpcell = Instantiate(cellType[map.color]); // <- 나중에 string name으로 바꿔야?
                GameObject tmpcell = Instantiate(cellMapOBJ.Cell[map.color]);
                tmpcell.GetComponent<HexCellPosition>().setInitPosition(map.x, map.z,map.w);
                tmpcell.name = "cell" + map.id;
                GameManager.data.mapCellid = map.id+1;
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



    //DEBUG----------------------------------------이 아래로 다 고쳐야함
    Dictionary<int, HexCoordinates> cellStoredPos = new Dictionary<int, HexCoordinates>();

    public void WarningCell(Pattern p)
    {
        //위치 세팅하면 저장해야함
        HexCoordinates RedZone = new HexCoordinates();

        //DEBUG - 이대로 두면 안 됨
        if (p.noteType == 1)
        {
            RedZone = pPosition;
        }

        //foreach(var coord in RedZone)
        {
            RedZone.plus(p.pivot.X, p.pivot.Z);
            //Debug.Log(RedZone.ToString());
            cellMaps.Get(RedZone).Warning();
            cellStoredPos.Add(p.id,RedZone);
        }
    }

    public void EnemyAttack(Pattern p)
    {
        //주의! 세팅된 위치가 아니라 다른 위치 쓰면 안됨

        Debug.Log(p.rhythmBeat.ToString() + ", " + cellStoredPos[p.id]);

        cellMaps.Get(cellStoredPos[p.id]).Damage(1);
    }
}
