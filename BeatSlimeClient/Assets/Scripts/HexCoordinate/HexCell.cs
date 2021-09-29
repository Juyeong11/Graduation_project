using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    public void Start()
    {

    }

    public void setInitPosition(int x, int z)
    {
        coordinates.setCoordinates(x, z);

        gameObject.transform.position = new Vector3(coordinates.X * 0.866f, 0f, coordinates.X * 0.5f + coordinates.Z * 1f);

    }
}
