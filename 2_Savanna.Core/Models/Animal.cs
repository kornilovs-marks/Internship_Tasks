using Savanna.Core.Interfaces;

namespace Savanna.Core.Models;

public class Animal
{
    public Guid Id { get; } = Guid.NewGuid();
    public AnimalKind Kind { get; }
    public Position Position { get; set; }
    public double Speed { get; }
    public int VisionRange { get; }
    public IAnimalBehavior Behavior { get; }
    public double MovementCredit { get; set; }

    // Constructor to initialize the animal with its properties
    public Animal(AnimalKind kind, Position position, double speed, int visionRange, IAnimalBehavior behavior)
    {
        Validate(speed, visionRange, kind, behavior);

        Kind = kind;
        Position = position;
        Speed = speed;
        VisionRange = visionRange;
        Behavior = behavior;
    }

    // Method to convert the Animal instance to an AnimalView
    public AnimalView ToView() => new(Id, Kind, Position, Speed, VisionRange);

    // Validate the parameters to ensure they are within acceptable ranges
    public void Validate(double speed, int visionRange, AnimalKind kind, IAnimalBehavior behavior)
    {
        if (!double.IsFinite(speed) || speed <= 0)
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be a positive finite number.");

        if (visionRange <= 0)
            throw new ArgumentOutOfRangeException(nameof(visionRange), "Vision range must be a positive number.");

        if (behavior.Kind != kind)
            throw new ArgumentException("Behavior kind does not match animal kind.", nameof(behavior));

    }
}
