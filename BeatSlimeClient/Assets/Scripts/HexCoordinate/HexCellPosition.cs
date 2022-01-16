using UnityEngine;

public class HexCellPosition : MonoBehaviour
{
    public HexCoordinates coordinates;
    public HexCoordinates preCoordinates;
    public HexDirection direction;
    float preBeatedTime;

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
        return new Vector3(coordinates.X * 0.866f, calculateWPosition(coordinates.W), coordinates.X * 0.5f + coordinates.Z * 1f);
    }

    public void reflectPosition()
    {
        //gameObject.transform.position = new Vector3(coordinates.X * 0.866f, calculateWPosition(coordinates.W) , coordinates.X * 0.5f + coordinates.Z * 1f);
    }

    static public float calculateWPosition(int w)
    {
        return w * 0.2f;
    }

    //여기서 Lerp
    public Vector3 calculatePlayerPosition()
    {
        int beatTime = GameManager.data.timeByBeat;

        float tick = LerpSquare((Time.time - preBeatedTime) * 1000f / beatTime);
        float newX;
        float newY;
        float newZ;

        newX = Mathf.Lerp(preCoordinates.X * 0.866f, coordinates.X * 0.866f, tick);
        newZ = Mathf.Lerp(preCoordinates.X * 0.5f + preCoordinates.Z, coordinates.X * 0.5f + coordinates.Z, tick);
        newY = SlimeWLerp(calculateWPosition(preCoordinates.W), calculateWPosition(coordinates.W), tick);

        if (tick >= 1f)
        {
            preCoordinates = coordinates;
        }

        return new Vector3(newX, newY, newZ);
    }

    public float LerpSquare(float tick)
    {
        if (tick < 0.3f)
            return 0f;
        else if (tick < 0.7f)
        {
            return (tick - 0.3f) * 2.5f;
        }
        else
            return 1f;
    }
    
    public void beat()
    {

    }

    public float SlimeWLerp(float a, float b, float t)
    {
        float skyHigh = (a+b)*0.5f + 2f;

        if (t <0.5f)
        {
            return Mathf.Lerp(a, skyHigh, t);
        }
        else
        {
            return Mathf.Lerp(skyHigh, b, t);
        }
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
