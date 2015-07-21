using System;

public struct Vector2i
{
	public int x, y;

	public Vector2i(int _x, int _y) { x = _x; y = _y; }

	public override string ToString()
	{
		return "{" + x + ", " + y + "}";
	}
}