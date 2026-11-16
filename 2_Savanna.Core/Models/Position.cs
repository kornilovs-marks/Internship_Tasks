namespace Savanna.Core.Models;

public readonly record struct Position(int x, int y)
{
    // Calculates the distance between this position and another position
    public int DistanceTo(Position other) => Math.Abs(x - other.x) + Math.Abs(y - other.y);

    // Adds two positions together, resulting in a new position
    public static Position operator +(Position left, Position right) => new Position(left.x + right.x, left.y + right.y);
}
