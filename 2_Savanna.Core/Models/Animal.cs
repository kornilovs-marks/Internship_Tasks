using Savanna.Core.Interfaces;

namespace Savanna.Core.Models;

public abstract class Animal
{
    public Guid Id { get; } = Guid.NewGuid();
    public AnimalKind Kind { get; }
    public Position Position { get; set; }
    public double Speed { get; }
    public int VisionRange { get; }
    public IAnimalBehavior Behavior { get; }
    public double MovementCredit { get; set; }

    protected Animal(AnimalKind kind, Position position, double speed, int visionRange, IAnimalBehavior behavior)
    {
        if (!double.IsFinite(speed) || speed <= 0 || visionRange < 0)
            throw new ArgumentException("Animal configuration is invalid.");

        Kind = kind;
        Position = position;
        Speed = speed;
        VisionRange = visionRange;
        Behavior = behavior;
    }

    public AnimalView ToView() => new(Id, Kind, Position, Speed, VisionRange);
}
