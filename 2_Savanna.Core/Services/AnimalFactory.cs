using Savanna.AnimalsBase.Factories;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Interfaces;
using Savanna.Core.Models;

namespace Savanna.Core.Services;

public class AnimalFactory(IEnumerable<AnimalFactoryBase> factories) : IAnimalFactory
{
    private readonly IReadOnlyDictionary<SpeciesId, AnimalFactoryBase> _factories =
        factories.ToDictionary(static factory => factory.Species.Id);

    public Animal Create(SpeciesId species, Position position) =>
        _factories.TryGetValue(species, out var factory)
            ? new Animal(factory.Species, position, factory.CreateBehavior())
            : throw new ArgumentOutOfRangeException(nameof(species), species, "No factory is registered.");
}
