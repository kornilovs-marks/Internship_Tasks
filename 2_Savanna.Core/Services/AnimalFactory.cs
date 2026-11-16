using Savanna.Core.Factories;
using Savanna.Core.Interfaces;
using Savanna.Core.Models;

namespace Savanna.Core.Services;

public class AnimalFactory(IEnumerable<AnimalFactoryBase> factories) : IAnimalFactory
{
    private readonly IReadOnlyDictionary<AnimalKind, AnimalFactoryBase> _factories =
        factories.ToDictionary(factory => factory.Kind);

    public Animal Create(AnimalKind kind, Position position) =>
        _factories.TryGetValue(kind, out var factory)
            ? factory.Create(position)
            : throw new ArgumentOutOfRangeException(nameof(kind), kind, "No factory is registered.");
}
