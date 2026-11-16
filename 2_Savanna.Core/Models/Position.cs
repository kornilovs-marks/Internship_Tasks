namespace Savanna.Core.Models;

public readonly record struct Position(int x, int y)
{
    public int DistanceTo(Position other) => Math.Abs(x - other.x) + Math.Abs(y - other.y);
    public static Position operator +(Position left, Position right) => new Position(left.x + right.x, left.y + right.y);
}
