using Savanna.AnimalsBase.Behaviors;
using Savanna.AnimalsBase.Interfaces;
using Savanna.AnimalsBase.Models;

namespace Savanna.Core.Behaviors;

public class LionBehavior(Random random) : IAnimalBehavior
{
    // Method to determine the next position for an antelope based on its current position and the world view
    public AnimalActions ChooseAction(AnimalView animal, IWorldView world)
    {
        var food = world.FindNearby(animal.Position, animal.VisionRange)
            .Where(other => other.Id != animal.Id && world.CanEat(animal.Species, other.Species))
            .OrderBy(other => animal.Position.DistanceTo(other.Position))
            .FirstOrDefault();

        if (food.Id == Guid.Empty)
            return new AnimalActions(Savanna.AnimalsBase.Behaviors.DirectionChoices.Wander(animal.Position, world, random));

        if (food.Position == animal.Position)
            return new AnimalActions(default, food.Id);

        var direction = Savanna.AnimalsBase.Behaviors.DirectionChoices.TowardOrAway(animal.Position, food.Position, world, random, away: false);
        return new AnimalActions(direction, food.Id);
    }
}
