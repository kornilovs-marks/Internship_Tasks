using Savanna.Core.Interfaces;
using Savanna.Core.Models;

namespace Savanna.Core.Behaviors;

public class LionBehavior(Random random) : IAnimalBehavior
{
    // Method to determine the next position for an antelope based on its current position and the world view
    public AnimalActions ChooseAction(AnimalView animal, IWorldView world)
    {
        var antelope = world.FindNearby(animal.Position, animal.VisionRange)
            .Where(other => other.Kind == AnimalKind.Antelope)
            .OrderBy(other => animal.Position.DistanceTo(other.Position))
            .FirstOrDefault();

        if (antelope.Id == Guid.Empty)
            return new AnimalActions(DirectionChoices.Wander(animal.Position, world, random));

        if (antelope.Position == animal.Position)
            return new AnimalActions(default, antelope.Id);

        var direction = DirectionChoices.TowardOrAway(animal.Position, antelope.Position, world, random, away: false);
        return new AnimalActions(direction, antelope.Id);
    }
}
