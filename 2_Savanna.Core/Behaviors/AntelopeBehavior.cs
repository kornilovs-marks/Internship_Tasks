using Savanna.Core.Interfaces;
using Savanna.Core.Models;

namespace Savanna.Core.Behaviors;

public class AntelopeBehavior(Random random) : IAnimalBehavior
{
    public AnimalKind Kind => AnimalKind.Antelope;

    // Method to determine the next position for an antelope based on its current position and the world view
    public AnimalActions ChooseAction(AnimalView animal, IWorldView world)
    {
        var lion = world.FindNearby(animal.Position, animal.VisionRange)
            .Where(other => other.Kind == AnimalKind.Lion)
            .OrderBy(other => animal.Position.DistanceTo(other.Position))
            .FirstOrDefault();

        var direction = lion.Id == Guid.Empty
            ? DirectionChoices.Wander(animal.Position, world, random)
            : DirectionChoices.TowardOrAway(animal.Position, lion.Position, world, random, away: true);
        
        return new AnimalActions(direction);
    }
}
