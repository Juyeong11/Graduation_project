using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum cellState
{
    Normal,
    Damaged,
    None,
}

public class Cell
{
    public GameObject obejct;
    public int x;
    public int y;
    public int z;

    public int duration = 0;
    public cellState state
    {
        get;
        set;
    }
    public string sprite;

    public Cell()
    {
        obejct = null;
        x = 1000;
        y = 1000;
        z = 1000;
        state = cellState.None;
        sprite = "none";
    }

    public Cell(GameObject cell, int x, int y,int z, cellState state, string sp)
    {
        obejct = cell;
        this.x = x;
        this.y = y;
        this.z = z;
        this.state = state;
        sprite = sp;
    }

    public (int,int,int) getCoordinate()
    {
        return (x, y, z);
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
            state = cellState.Normal;
        }
        else
        {
            duration--;
        }
    }
}

public class CellMap
{
    public List<Cell> cellMaps = new List<Cell>();

    public void Add(GameObject cell,int x, int y,int z)
    {
        cellMaps.Add(new Cell(cell, x, y, z, cellState.Normal, "default"));
    }
    public void Add(GameObject cell, int x, int y, int z, cellState state, string name)
    {
        cellMaps.Add(new Cell(cell, x, y, z, state, name));
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

    public Cell Get(int x, int z)
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

        Debug.LogError(">Invalid coordinate<");
        return new Cell();
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
        return new Cell();
    }

}

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

    //Cell 종류
    public List<GameObject> cellType;
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
            if(!GameManager.data.Net.isOnline)
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
        RedZones = new List<List<HexCoordinates>>();

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
                        GameObject tmpcell = Instantiate(cellType[0]); // <- 나중에 string name으로 바꿔야?
                        tmpcell.GetComponent<HexCellPosition>().setInitPosition(x, z);
                        tmpcell.name = "cell"+x+y+z;
                        tmpcell.transform.parent = gameObject.transform;
                        cellMaps.Add(tmpcell,x,y,z);
                    }
                }
            }
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
