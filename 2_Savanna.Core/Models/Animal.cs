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
    public double Health { get; private set; }
    public double MaxHealth { get; }
    public bool IsAlive => Health > 0;

    protected Animal(AnimalKind kind, Position position, double speed, int visionRange, double health, IAnimalBehavior behavior)
    {
        if (!double.IsFinite(speed) || speed <= 0 || visionRange < 0)
            throw new ArgumentException("Animal configuration is invalid.");

        if (!double.IsFinite(health) || health <= 0)
            throw new ArgumentOutOfRangeException(nameof(health), health, "Health must be a positive finite number.");

        Kind = kind;
        Position = position;
        Speed = speed;
        VisionRange = visionRange;
        Behavior = behavior;
        Health = health;
        MaxHealth = health;
        Behavior = behavior;
    }

    public AnimalView ToView() => new(Id, Kind, Position, Speed, VisionRange, Health);

    // Reduces health, never below zero
    public void LoseHealth(double amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        Health = Math.Max(Health - amount, 0);
    }

    // Restores health, never above the health the animal was born with
    public void Heal(double amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        Health = Math.Min(Health + amount, MaxHealth);
    }
}
