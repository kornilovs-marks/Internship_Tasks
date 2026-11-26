using Savanna.Core.Configuration;
using Savanna.Core.Constants;
using Savanna.Core.Interfaces;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Models;

namespace Savanna.Core.Services;

public class GameEngine(GameWorld world, IAnimalFactory factory, Random random, GameOptions options, AnimalMovement movement, AnimalBirth births)
{
    private readonly IAnimalFactory _factory = factory;
    private readonly Random _random = random;
    public GameWorld World { get; } = world;
    private readonly GameOptions _options = options;
    private readonly AnimalMovement _movement = movement;
    private readonly AnimalBirth _births = births;


    // Method to add a new animal of a specified kind to the game world at a random unoccupied position
    public bool TryAddAnimal(SpeciesId species)
    {
        var freeCells = FindFreeCells();
        
        if(freeCells.Count == 0)
            return false;

        var position = freeCells[_random.Next(freeCells.Count)];
        world.Add(_factory.Create(species, position));
        return true;
    }

    // Method to advance the game state by a specified elapsed time, updating animal positions and resolving interactions
    public void Tick(TimeSpan elapsed)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(elapsed, TimeSpan.Zero);

        var plans = PlanActions();

        foreach(var plan in plans)
            _movement.Advance(plan.Animal, plan.Action.direction, elapsed.TotalSeconds);

        ResolveCatches(plans);
        RemoveDead();
        _births.AdvanceRounds(elapsed);
    }

    // Resolves all catched animals
    private void ResolveCatches(MovePlan[] plans)
    {
        var animalsByCell = world.Animals
            .GroupBy(static antelope => antelope.Position)
            .ToDictionary(static group => group.Key, static group => group.ToList<Animal>());

        foreach (var plan in plans)
        {
            var hunter = plan.Animal;

            if (!animalsByCell.TryGetValue(hunter.Position, out var occupants) || !occupants.Contains(hunter))
                continue;

            var prey = occupants.Where(other => other != hunter && hunter.CanEat(other)).ToList();
            if (prey.Count == 0)
                continue;

            var meal = prey.Find(other => other.Id == plan.Action.targetId) ?? prey[0];

            occupants.Remove(meal);
            World.Remove(meal);
            hunter.Heal(_options.HealthGainOnEat);
        }
    }

    // Death: removes every animal whose health has run out
    private void RemoveDead() => world.RemoveWhere(static animal => !animal.IsAlive);

    // Lists every cell that no animal currently occupies
    private List<Position> FindFreeCells()
    {
        var occupied = world.Animals.Select(static animal => animal.Position).ToHashSet();
        var freeCells = new List<Position>(world.Width * world.Height - occupied.Count);

        for (var y = 0; y < world.Height; y++)
        {
            for (var x = 0; x < world.Width; x++)
            {
                var cell = new Position(x, y);
                if (!occupied.Contains(cell))
                    freeCells.Add(cell);
            }
        }

        return freeCells;
    }

    // Every animal decides from the same frozen snapshot, so list order doesn't matter
    private MovePlan[] PlanActions()
    {
        var snapshot = world.Snapshot();

        return world.Animals
            .Select(animal => new MovePlan(animal, animal.Behavior.ChooseAction(animal.ToView(), snapshot)))
            .ToArray();
    }

    private readonly record struct MovePlan(Animal Animal, AnimalActions Action);
}
