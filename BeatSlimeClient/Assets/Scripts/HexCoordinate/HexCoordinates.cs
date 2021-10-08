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

	public HexCoordinates(int x, int z)
	{
		this.x = x;
		this.z = z;
	}
	public static HexCoordinates FromOffsetCoordinates(int x, int z)
	{
		return new HexCoordinates(x, z);
	}

	public void setCoordinates(int x, int z)
    {
		this.x = x;
		this.z = z;
	}

	public void plus(int x, int z)
    {
		this.x += x;
		this.z += z;
    }

	public override string ToString()
	{
		return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
	}

	public string ToStringOnSeparateLines()
	{
		return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
	}
}