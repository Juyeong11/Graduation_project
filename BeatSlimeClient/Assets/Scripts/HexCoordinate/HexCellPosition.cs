using UnityEngine;

public class HexCellPosition : MonoBehaviour
{
    public HexCoordinates coordinates;
    public HexCoordinates preCoordinates;
    public HexDirection direction;

    public float landOffsetX = 0;
    public float landOffsetY = 0;
    public float landOffsetZ = 0;
    public float landOffsetRotate = 0;

    public float preBeatedTime;

    public void landOffSetter(float x, float y, float z, float r)
    {
        landOffsetX = x;
        landOffsetY = y;
        landOffsetZ = z;
        landOffsetRotate = r;

        gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, landOffsetRotate, 0));
    }
    public void setInitPosition(int x, int z,int w=0)
    {
        preCoordinates.setCoordinates(x, z, w);
        coordinates.setCoordinates(x, z,w);

        gameObject.transform.localPosition = getRealPosition();

    }

    static public Vector3 getRealPosition(float x, float z, int w)
    {
        return new Vector3(x * 0.866f, calculateWPosition(w), x * 0.5f + z * 1f);
    }
    public Vector3 getRealPosition()
    {
        return new Vector3(coordinates.X * 0.866f + landOffsetX,
            calculateWPosition(coordinates.W) + landOffsetY,
            coordinates.X * 0.5f + coordinates.Z * 1f + landOffsetZ);
    }

    public void reflectPosition()
    {
        if (coordinates.X + coordinates.Y + coordinates.Z != 0)
        {
            Debug.LogError("reflectPosition checkSum error!!");
        }
        gameObject.transform.position = new Vector3(coordinates.X * 0.866f, calculateWPosition(coordinates.W) , coordinates.X * 0.5f + coordinates.Z * 1f);
    }

    static public float calculateWPosition(int w)
    {
        return w * 0.2f;
    }
    public void beat()
    {

    }

    public void setDirection(byte dir)
    {
        switch(dir)
        {
            case (byte)Protocol.DIR.DOWN:
                gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 60f, 0f));
                break;
            case (byte)Protocol.DIR.RIGHTDOWN:
                gameObject.transform.localRotation = Quaternion.Euler(new Vector3( 0f, 120f, 0f));
                break;
            case (byte)Protocol.DIR.LEFTUP:
                gameObject.transform.localRotation = Quaternion.Euler(new Vector3( 0f, 180f, 0f));
                break;
            case (byte)Protocol.DIR.UP:
                gameObject.transform.localRotation = Quaternion.Euler(new Vector3( 0f, -120f, 0f));
                break;
            case (byte)Protocol.DIR.RIGHTUP:
                gameObject.transform.localRotation = Quaternion.Euler(new Vector3( 0f, -60f, 0f));
                break;
            case (byte)Protocol.DIR.LEFTDOWN:
                gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                break;

        }
        direction = (HexDirection)dir;

    }
    public void plus(int x, int y, int z,int w=0)
    {
        preBeatedTime = Time.time;
        if (x+y+z != 0)
        {
            Debug.LogError("HexPlus Error (x+y+z != 0)");
        }
        preCoordinates = coordinates;
        coordinates.plus(x, z, w);
        //reflectPosition();
    }
    public void SetPosition(int x, int y, int z, int w=0)
    {
        preBeatedTime = Time.time;

        if (x + y + z != 0)
        {
            Debug.LogError("HexSetting Error (x+y+z != 0)");
        }
        preCoordinates = coordinates;
        coordinates.setCoordinates(x, z, w);
        //reflectPosition();
    }
    public (int,int,int,int) Get()
    {
        return (coordinates.X, coordinates.Y, coordinates.Z, coordinates.W);
    }

    public void setCellPositionByPivotTransform()
    {
        //enemy용, 피벗에서 가장 가까운 HexCell좌표를 찾아서 Coordinate를 그 좌표로 지정
    }
}
