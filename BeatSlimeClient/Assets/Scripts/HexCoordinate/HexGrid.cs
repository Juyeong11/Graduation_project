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
    public cellState state
    {
        get;
    }
    public string sprite;

    public Cell()
    {
        obejct = null;
        x = 0;
        y = 0;
        z = 0;
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

        //Debug.LogError(">Invalid coordinate<");
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

    public override string ToString()
    {
        return "override this!";
    }
    public void Start()
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

    public void EnemyAttack(Pattern p)
    {
        Debug.Log(p.rhythmBeat.ToString() + ", " + GameManager.data.nowBeat.ToString() + " " + "Damage!");
    }
}
