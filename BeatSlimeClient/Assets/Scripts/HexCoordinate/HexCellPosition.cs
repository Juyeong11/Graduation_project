using UnityEngine;

public class HexCellPosition : MonoBehaviour
{
    public HexCoordinates coordinates;

    public void setInitPosition(int x, int z)
    {
        coordinates.setCoordinates(x, z);

        gameObject.transform.localPosition = new Vector3(coordinates.X * 0.866f, 0f, coordinates.X * 0.5f + coordinates.Z * 1f);
    }

    public void refelectPosition()
    {
        gameObject.transform.localPosition = new Vector3(coordinates.X * 0.866f, gameObject.transform.localPosition.y, coordinates.X * 0.5f + coordinates.Z * 1f);
    }

    public void plus(int x, int y, int z)
    {
        if (x+y+z != 0)
        {
            Debug.LogError("HexPlus Error (x+y+z != 0)");
        }
        coordinates.plus(x, z);
        refelectPosition();
    }
    public void SetPosition(int x, int y, int z)
    {
        coordinates.setCoordinates(x, z);
        refelectPosition();
    }
    public (int,int,int) Get()
    {
        return (coordinates.X, coordinates.Y, coordinates.Z);
    }

    public void setCellPositionByPivotTransform()
    {
        //enemy용, 피벗에서 가장 가까운 HexCell좌표를 찾아서 Coordinate를 그 좌표로 지정
    }
}
