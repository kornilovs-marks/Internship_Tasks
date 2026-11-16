using Savanna.Core.Models;
using Savanna.Core.Interfaces;
using Savanna.Core.Configuration;

namespace Savanna.Core.Services;

public class AnimalFactory : IAnimalFactory
{
    private GameOptions _options;
    private IReadOnlyDictionary<AnimalKind, IAnimalBehavior> _behaviors;

    // Constructor for the AnimalFactory class that takes GameOptions and an inteface IAnimalBehavior
    public AnimalFactory(GameOptions options, IEnumerable<IAnimalBehavior> behaviors)
    {
        _options = options;
        _behaviors = behaviors.ToDictionary(behavior => behavior.Kind);
    }

    // Method to create an animal of a specific kind at a given position
    public Animal Create(AnimalKind kind, Position position)
    {
        if(!_behaviors.TryGetValue(kind, out var behavior))
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "No behavior is registered.");

        return kind switch
        {
            AnimalKind.Antelope => new Animal(kind, position, _options.AntelopeSpeed, _options.AntelopeVision, behavior),
            AnimalKind.Lion => new Animal(kind, position, _options.LionSpeed, _options.LionVision, behavior),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown animal kind.")
        };
    }
}
