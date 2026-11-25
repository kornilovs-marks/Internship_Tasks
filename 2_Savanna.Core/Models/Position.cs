namespace Savanna.Core.Models;

public readonly record struct Position(int x, int y)
{
    // Number of cells from (0, 0); a single step up, down, left or right has length 1
    public int Length => Math.Abs(x) + Math.Abs(y);

    // Checks whether this position lies inside a field of the given size
    public bool IsInside(int width, int height) => x >= 0 && x < width && y >= 0 && y < height;

    // Calculates the distance between this position and another position
    public int DistanceTo(Position other) => Math.Abs(x - other.x) + Math.Abs(y - other.y);

    // Adds two positions together, resulting in a new position
    public static Position operator +(Position left, Position right) => new Position(left.x + right.x, left.y + right.y);
}
