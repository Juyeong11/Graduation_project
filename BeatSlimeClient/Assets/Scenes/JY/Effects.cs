using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsCell
{
    public int x;
    public int y;
    public int z;

    EffectsCell() { }
    public EffectsCell(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

public class Effects
{
    List<List<EffectsCell>> TileEffects = new List<List<EffectsCell>>();

    List<EffectsCell> HexPattern3 = new List<EffectsCell>();
    List<EffectsCell> HexPattern4 = new List<EffectsCell>();

    public Effects()
    {
        HexPattern3.Add(new EffectsCell(0, -1, 1));
        HexPattern3.Add(new EffectsCell(1, -1, 0));
        HexPattern3.Add(new EffectsCell(1, 0, -1));
        HexPattern3.Add(new EffectsCell(0, 1, -1));
        HexPattern3.Add(new EffectsCell(-1, 1, 0));
        HexPattern3.Add(new EffectsCell(-1, 0, 1));
        HexPattern3.Add(new EffectsCell(+1, +1, -2));
        HexPattern3.Add(new EffectsCell(-1, -1, +2));
        HexPattern3.Add(new EffectsCell(+2, -2, 0));
        HexPattern3.Add(new EffectsCell(-2, +2, 0));

        HexPattern4.Add(new EffectsCell(0, -1, +1));
        HexPattern4.Add(new EffectsCell(0, -2, +2));
        HexPattern4.Add(new EffectsCell(-1, 0, +1));
        HexPattern4.Add(new EffectsCell(-2, 0, +2));
        HexPattern4.Add(new EffectsCell(+1, 0, -1));
        HexPattern4.Add(new EffectsCell(+2, 0, -2));
        HexPattern4.Add(new EffectsCell(0, +1, -1));
        HexPattern4.Add(new EffectsCell(0, +2, -2));

        TileEffects.Add(new List<EffectsCell>());   //패턴 1 만들면 수정
        TileEffects.Add(new List<EffectsCell>());   //패턴 2 만들면 수정
        TileEffects.Add(HexPattern3);
        TileEffects.Add(HexPattern4);
    }

    public List<EffectsCell> GetPattern(int num)
    {
        return TileEffects[num-1];  //자연수로 배열 접근하려고
    }


}
