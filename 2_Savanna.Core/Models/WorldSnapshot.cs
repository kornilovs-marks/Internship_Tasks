using Savanna.Core.Interfaces;

namespace Savanna.Core.Models;

public class WorldSnapshot : IWorldView
{
    public int Width { get; }
    public int Height { get; }
    public IReadOnlyList<AnimalView> Animals { get; }

    public WorldSnapshot(int width, int height, IEnumerable<AnimalView> animals)
    {
        Width = width;
        Height = height;
        Animals = Array.AsReadOnly(animals.ToArray());
    }

    // Checks if a given position is within the bounds of the grid
    public bool Contains(Position position) => position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;

    // Finds all animals within a certain range of a given position
    public IEnumerable<AnimalView> FindNearby(Position center, int range) => Animals.Where(a => a.Position.DistanceTo(center) <= range);
}
