using Savanna.Core.Configuration;
using Savanna.Core.Constants;
using Savanna.Core.Interfaces;
using Savanna.Core.Models;

namespace Savanna.Core.Services;

public class GameEngine(GameWorld world, IAnimalFactory factory, Random random, GameOptions options)
{
    private readonly IAnimalFactory _factory = factory;
    private readonly Random _random = random;
    public GameWorld World { get; } = world;
    private readonly GameOptions _options = options;
    private readonly AnimalMovement _movement = new(world, options);
    private readonly AnimalBirth _births = new(world, factory, random, options);


    // Method to add a new animal of a specified kind to the game world at a random unoccupied position
    public bool TryAddAnimal(AnimalKind kind)
    {
        var freeCells = FindFreeCells();
        
        if(freeCells.Count == 0)
            return false;

        var position = freeCells[_random.Next(freeCells.Count)];
        world.Add(_factory.Create(kind, position));
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
        var antelopesByCell = world.Animals
            .OfType<Antelope>()
            .GroupBy(static antelope => antelope.Position)
            .ToDictionary(static group => group.Key, static group => group.ToList<Animal>());

        foreach (var plan in plans)
        {
            if (plan.Animal is not Lion lion)
                continue;

            if (!antelopesByCell.TryGetValue(lion.Position, out var prey))
                continue;

            var meal = prey.Find(antelope => antelope.Id == plan.Action.targetId) ?? prey[0];

            prey.Remove(meal);
            if (prey.Count == 0)
                antelopesByCell.Remove(lion.Position);

            world.Remove(meal);
            lion.Heal(_options.LionHealthGainOnEat);
        }
    }

    // Death: removes every animal whose health has run out
    private void RemoveDead()
    {
        var dead = world.Animals.Where(static animal => !animal.IsAlive).ToList();

        foreach (var animal in dead)
            world.Remove(animal);
    }

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
