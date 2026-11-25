using Savanna.Core.Constants;
using Savanna.Core.Interfaces;
using Savanna.Core.Models;

namespace Savanna.Core.Services;

public class GameEngine(GameWorld world, IAnimalFactory factory, Random random)
{
    private readonly IAnimalFactory _factory = factory;
    private readonly Random _random = random;
    public GameWorld World { get; } = world;

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
            Advance(plan.Animal, plan.Action.direction, elapsed.TotalSeconds);

        ResolveCatches(plans);
    }

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
        }
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

    // Gives the animal its share of time and spends it on steps
    private void Advance(Animal animal, Position direction, double seconds)
    {
        animal.MovementCredit += animal.Speed * seconds;

        if (direction.Length == 1)
            Walk(animal, direction);

        animal.MovementCredit = Math.Min(animal.MovementCredit, GameDefaults.MaxStoredCredit);
    }

    // Takes one step per full credit until the credit runs out or the edge is reached
    private void Walk(Animal animal, Position direction)
    {
        while (animal.MovementCredit >= GameDefaults.StepCost)
        {
            animal.MovementCredit -= GameDefaults.StepCost;

            var next = animal.Position + direction;
            if (!world.Contains(next))
                return;

            world.Move(animal, next);
        }
    }

    private readonly record struct MovePlan(Animal Animal, AnimalActions Action);
}
