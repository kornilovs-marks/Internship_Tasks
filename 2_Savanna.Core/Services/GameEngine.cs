

using Savanna.Core.Interfaces;
using Savanna.Core.Models;

namespace Savanna.Core.Services;

public class GameEngine(GameWorld world, IAnimalFactory factory, Random random)
{
    public GameWorld World => world;

    // Method to add a new animal of a specified kind to the game world at a random unoccupied position
    public bool TryAddAnimal(AnimalKind kind)
    {
        var occupied = world.Animals.Select(animal => animal.Position).ToHashSet();
        if(occupied.Count >= world.Width * world.Height)
            return false;

        Position position;
        do
        {
            position = new Position(random.Next(world.Width), random.Next(world.Height));
        }
        while (occupied.Contains(position));

        world.Add(factory.Create(kind, position));
        return true;
    }

    // Method to advance the game state by a specified elapsed time, updating animal positions and resolving interactions
    public void Tick(TimeSpan elapsed)
    {
        if(elapsed <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(elapsed), elapsed, "Elapsed time must be positive.");
        var snapshot = world.Snapshot();
        var plans = world.Animals
            .Select(animal => (Animal: animal, Action: animal.Behavior.ChooseAction(animal.ToView(), snapshot)))
            .ToArray();

        foreach(var (animal, action) in plans)
        {
            animal.MovementCredit += animal.Speed * elapsed.TotalSeconds;
            var direction = action.direction;

            if (Math.Abs(direction.x) + Math.Abs(direction.y) != 1)
            {
                animal.MovementCredit = Math.Min(animal.MovementCredit, 1); 
                continue;
            }

            while(animal.MovementCredit >= 1)
            {
                var next = animal.Position + direction;
                animal.MovementCredit -= 1;
                if(!world.Contains(next))
                    break;
                world.Move(animal, next);
            }
            animal.MovementCredit = Math.Min(animal.MovementCredit, 1);
        }
        ResolveCatches(plans);
    }

    private void ResolveCatches((Animal Animal, AnimalActions Action)[] plans)
    {
        var antelopes = world.Animals.Where(animal => animal.Kind == AnimalKind.Antelope).ToList();
        foreach (var (lion, action) in plans.Where(plan => plan.Animal.Kind == AnimalKind.Lion))
        {
            var atPosition = antelopes.Where(animal => animal.Position == lion.Position).ToList();
            var meal = atPosition.FirstOrDefault(animal => animal.Id == action.targetId)
                ?? atPosition.FirstOrDefault();

            if (meal is null)
                continue;

            world.Remove(meal);
            antelopes.Remove(meal);
        }
    }
}
