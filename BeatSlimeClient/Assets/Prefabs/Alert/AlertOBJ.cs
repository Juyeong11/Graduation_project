using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AlertOBJ", menuName = "Scriptable Object/Alert", order = int.MaxValue)]
public class AlertOBJ : ScriptableObject
{
    [SerializeField]
    [TextArea]
    private List<string> al;

    public List<string> Data { get { return al; } }


}
