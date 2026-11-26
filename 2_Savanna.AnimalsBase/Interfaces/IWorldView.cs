using Savanna.AnimalsBase.Models;

namespace Savanna.AnimalsBase.Interfaces;

public interface IWorldView
{
    int Width { get; }
    int Height { get; }
    IReadOnlyList<AnimalView> Animals { get; }
    IEnumerable<AnimalView> FindNearby(Position center, int range);
    bool Contains(Position position);
    bool CanEat(SpeciesId eater, SpeciesId food);
}
