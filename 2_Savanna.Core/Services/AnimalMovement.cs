using Savanna.Core.Configuration;
using Savanna.Core.Constants;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Models;

namespace Savanna.Core.Services;

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

    // One animal per cell, lion is an exception
    private bool IsBlocked(Animal animal, Position cell) =>
        _world.Animals.Any(other => other.Position == cell && other != animal && !animal.CanEat(other));
}
