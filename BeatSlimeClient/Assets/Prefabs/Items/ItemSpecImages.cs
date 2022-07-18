using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Item Spec Images", menuName = "Scriptable Object/Item Spec Images", order = int.MaxValue)]
public class ItemSpecImages : ScriptableObject
{
    [SerializeField]
    private Sprite[] img;
    public Sprite[] Img { get { return img; } }


}