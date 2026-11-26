using Savanna.AnimalsBase.Models;
using Savanna.AnimalsBase.Interfaces;
using Savanna.Core.Services;

namespace Savanna.Core.Models;

public class WorldSnapshot : IWorldView
{
    private SpeciesRegistry _species;
    public int Width { get; }
    public int Height { get; }
    public IReadOnlyList<AnimalView> Animals { get; }

    public WorldSnapshot(int width, int height, IEnumerable<AnimalView> animals, SpeciesRegistry species)
    {
        Width = width;
        Height = height;
        Animals = Array.AsReadOnly(animals.ToArray());
        _species = species;
    }

    // Checks if a given position is within the bounds of the grid
    public bool Contains(Position position) => position.IsInside(Width, Height);

    // Finds all animals within a certain range of a given position
    public IEnumerable<AnimalView> FindNearby(Position center, int range) => Animals.Where(animal => animal.Position.DistanceTo(center) <= range);

    public bool CanEat(SpeciesId eater, SpeciesId food) => _species.CanEat(eater, food);
}
