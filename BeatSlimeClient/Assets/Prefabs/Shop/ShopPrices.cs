using UnityEngine;


[CreateAssetMenu(fileName = "ShopPrices", menuName = "Scriptable Object/ShopPrices", order = int.MaxValue)]
public class ShopPrices : ScriptableObject
{
    [SerializeField]
    private int[] skill1Prices;
    public int[] Skill1Prices { get { return skill1Prices; } }

    [SerializeField]
    private int[] skill2Prices;
    public int[] Skill2Prices { get { return skill2Prices; } }

    [SerializeField]
    private int[] skill3Prices;
    public int[] Skill3Prices { get { return skill3Prices; } }

}
