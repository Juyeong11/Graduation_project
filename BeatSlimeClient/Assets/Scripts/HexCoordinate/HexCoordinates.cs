using UnityEngine;

public enum HexDirection
{
	LeftUp,
	Up,
	RightUp,
	LeftDown,
	Down,
	RightDown,
}

[System.Serializable]
public struct HexCoordinates
{
	[SerializeField]
	private int x, z;
	[SerializeField]
	private int w;
	public int X {
		get
		{
			return x;
		}
        set { x = value; }
	}

	public int Z {
		get
		{
			return z;
		}
		set { z = value; }
	}

	public int Y
	{
		get
		{
			return -X - Z;
		}
	}

	public int W
    {
        get
        {
			return w;
        }
		set { w = value; }
    }

	public HexCoordinates(int x, int z, int w=0)
	{
		this.x = x;
		this.z = z;
		this.w = w;
	}

	public static HexCoordinates FromOffsetCoordinates(int x, int z, int w=0)
	{
		return new HexCoordinates(x, z, w);
	}
	public Vector3 getRealPosition()
	{
		return new Vector3(this.x * 0.866f, this.w * 0.2f, this.x * 0.5f + this.z * 1f);
	}
	public void setCoordinates(int x, int z, int w=0)
    {
		this.x = x;
		this.z = z;
		this.w = w;
	}

	public void plus(int x, int z, int w=0)
    {
		this.x += x;
		this.z += z;
		this.w += w;
    }

	public override string ToString()
	{
		return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ", " + W.ToString() + ")";
	}

	public string ToStringOnSeparateLines()
	{
		return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString() + "\n\n" + W.ToString();
	}
}