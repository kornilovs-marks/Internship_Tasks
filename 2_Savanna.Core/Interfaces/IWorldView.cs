using Savanna.Core.Models;

namespace Savanna.Core.Interfaces;

public interface IWorldView
{
    int Width { get; }
    int Height { get; }
    IReadOnlyList<AnimalView> Animals { get; }
    IEnumerable<AnimalView> FindNearby(Position center, int range);
    bool Contains(Position position);
}
