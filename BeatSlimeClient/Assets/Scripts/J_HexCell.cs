using UnityEngine;

public class J_HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;

    public Vector3 DesPos;
    public void Start()
    {
        setInitPosition(0, 0);
    }

    public void setInitPosition(int x, int z)
    {
        coordinates.setCoordinates(x, z);

        gameObject.transform.localPosition = new Vector3(coordinates.X * 0.866f, 0f, coordinates.X * 0.5f + coordinates.Z * 1f);
    }



    public void plus(int x, int y, int z)
    {
        if (x + y + z != 0)
        {
            Debug.LogError("HexPlus Error (x+y+z != 0)");
        }
        coordinates.plus(x, z);

        DesPos = new Vector3(coordinates.X * 0.866f, gameObject.transform.localPosition.y, coordinates.X * 0.5f + coordinates.Z * 1f);
    }
}
