using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cell Scriptable Object", menuName = "Scriptable Object/Cell Scriptable Object", order = int.MaxValue)]
public class CellScriptableObject : ScriptableObject
{
    [SerializeField]
    private GameObject[] cells;
    public GameObject[] Cells { get { return cells; } }


    [SerializeField]
    private GameObject[] lands;
    public GameObject[] Lands { get { return lands; } }


}