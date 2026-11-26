using Savanna.AnimalsBase.Models;
using Savanna.AnimalsBase.Interfaces;
using Savanna.AnimalsBase.Behaviors;

namespace Savanna.Core.Behaviors;

public class AntelopeBehavior(Random random) : IAnimalBehavior
{
    // Method to determine the next position for an antelope based on its current position and the world view
    public AnimalActions ChooseAction(AnimalView animal, IWorldView world)
    {
        var threat = world.FindNearby(animal.Position, animal.VisionRange)
            .Where(other => world.CanEat(other.Species, animal.Species))
            .OrderBy(other => animal.Position.DistanceTo(other.Position))
            .FirstOrDefault();

        var direction = threat.Id == Guid.Empty
            ? Savanna.AnimalsBase.Behaviors.DirectionChoices.Wander(animal.Position, world, random)
            : Savanna.AnimalsBase.Behaviors.DirectionChoices.TowardOrAway(animal.Position, threat.Position, world, random, away: true);
        
        return new AnimalActions(direction);
    }
}
