using UnityEngine;


[CreateAssetMenu(fileName = "Class Image Object", menuName = "Scriptable Object/Class Image Object", order = int.MaxValue)]
public class ClassImageObject : ScriptableObject
{
    [SerializeField]
    private Sprite[] classSprites;
    public Sprite[] ClassSprites { get { return classSprites; } }


}
