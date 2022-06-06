using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CellMapOBJ", menuName = "Scriptable Object/CellMap", order = int.MaxValue)]
public class CellMapOBJ : ScriptableObject
{
    [SerializeField]
    private List<GameObject> cellTypes;

    [SerializeField]
    private List<GameObject> landTypes;


    public List<GameObject> Cell { get { return cellTypes; } }

    public List<GameObject> Land { get { return landTypes; } }

}
