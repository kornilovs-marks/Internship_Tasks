using Savanna.Core.Configuration;
using Savanna.Core.Constants;
using Savanna.Core.Models;

namespace Savanna.Core.Services;

// Moves animals across the world: spends movement credit, charges health per step and stops at walls or occupied cells
public class AnimalMovement(GameWorld world, GameOptions options)
{
    private readonly GameWorld _world = world;
    private readonly GameOptions _options = options;

    // Gives the animal its share of time and spends it on steps
    public void Advance(Animal animal, Position direction, double seconds)
    {
        animal.MovementCredit += animal.Speed * seconds;

        if (direction.Length == 1)
            Walk(animal, direction);

        animal.MovementCredit = Math.Min(animal.MovementCredit, GameDefaults.MaxStoredCredit);
    }

    // Takes one step per full credit until the credit runs out or the way is blocked; every step costs health
    private void Walk(Animal animal, Position direction)
    {
        while (animal.MovementCredit >= GameDefaults.StepCost)
        {
            animal.MovementCredit -= GameDefaults.StepCost;

            var next = animal.Position + direction;
            if (!_world.Contains(next) || IsBlocked(animal, next))
                return;

            _world.Move(animal, next);
            animal.LoseHealth(_options.MoveHealthCost);
        }
    }

    // One animal per cell; the only exception is a lion stepping onto an antelope to eat it
    private bool IsBlocked(Animal animal, Position cell)
    {
        var occupants = _world.Animals.Where(other => other.Position == cell).ToList();

        if (occupants.Count == 0)
            return false;

        return animal is not Lion || occupants.Any(static other => other is not Antelope);
    }
}
