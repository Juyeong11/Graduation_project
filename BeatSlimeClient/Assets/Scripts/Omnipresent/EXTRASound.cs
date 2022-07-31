using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Extra Sound Object", menuName = "Scriptable Object/Extra Sound", order = int.MaxValue)]
public class EXTRASound : ScriptableObject
{

    [SerializeField]
    private AudioClip[] sounds;
    public AudioClip[] Sounds { get { return sounds; } }
}
