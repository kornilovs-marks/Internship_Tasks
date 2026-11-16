namespace Savanna.Core.Models;

public record struct AnimalActions(Position direction, Guid? targetId = null)
{
    // Represents an action where the animal does not move
    public static AnimalActions Stay => new(new Position(0, 0));
}