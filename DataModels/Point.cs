using System;

public struct Point
{
	public int X { get; }
	public int Y { get; }
	public int Z { get; }

	public Point(int x, int y, int z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public Point (int oneTurnAngle)
	{
		X = 0;
		Y = 0;
		Z = 0;
		switch (oneTurnAngle)
		{
			case 0:
				X=1;
				break;
			case 90:
				Y=1;
				break;
			case 180:
				X=-1;
				break;
			default:
				Y=-1;
				break;
		}
	}

	public int L1(Point point, bool plane=true)
	{
		if(plane)
			return Math.Abs(X - point.X) + Math.Abs(Y - point.Y);
		else
			return Math.Abs(X - point.X) + Math.Abs(Y - point.Y) + Math.Abs(Z - point.Z);
	}

	public bool IsInside(Point p1, Point p2)
	{
		return (this >= p1) && (this <= p2);
	}

	public override string ToString()
	{
		return String.Format("X: {0}, Y: {1}, Z: {2}", X, Y, Z);
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(Point p1, Point p2)
	{
		return (p1.X == p2.X) && (p1.Y == p2.Y) && (p1.Z == p2.Z);
	}
	public static bool operator !=(Point p1, Point p2)
	{
		return (p1.X != p2.X) || (p1.Y != p2.Y) || (p1.Z != p2.Z);
	}
	public static Point operator *(Point p, int factor)
	{
		return new Point(p.X * factor, p.Y * factor, p.Z * factor);
	}
	public static Point operator +(Point p1, Point p2)
	{
		return new Point(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
	}
	public static Point operator -(Point p1, Point p2)
	{
		return new Point(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
	}
	public static bool operator >=(Point p1, Point p2)
	{
		return (p1.X >= p2.X) && (p1.Y >= p2.Y) && (p1.Z >= p2.Z);
	}
	public static bool operator <=(Point p1, Point p2)
	{
		return (p1.X <= p2.X) && (p1.Y <= p2.Y) && (p1.Z <= p2.Z);
	}
}