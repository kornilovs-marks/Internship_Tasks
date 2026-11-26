using Savanna.AnimalsBase.Interfaces;
using Savanna.AnimalsBase.Models;

namespace Savanna.Core.Models;

public class Animal
{
    public Guid Id { get; } = Guid.NewGuid();
    public SpeciesDescriptor Species { get; }
    public SpeciesId SpeciesId => Species.Id;
    public Position Position { get; set; }
    public double Speed => Species.Speed;
    public int VisionRange => Species.VisionRange;
    public IAnimalBehavior Behavior { get; }
    public double MovementCredit { get; set; }
    public double Health { get; private set; }
    public double MaxHealth { get; }
    public bool IsAlive => Health > 0;

    public Animal(SpeciesDescriptor species, Position position, IAnimalBehavior behavior)
    {
        ArgumentNullException.ThrowIfNull(species);
        ArgumentNullException.ThrowIfNull(behavior);

        if (!double.IsFinite(species.Speed) || species.Speed <= 0 || species.VisionRange < 0)
            throw new ArgumentException("Animal configuration is invalid.");

        if (!double.IsFinite(species.Health) || species.Health <= 0)
            throw new ArgumentOutOfRangeException(nameof(species.Health), species.Health, "Health must be a positive finite number.");

        Species = species;
        Position = position;
        Behavior = behavior;
        Health = Species.Health;
        MaxHealth = Species.Health;
    }

    public AnimalView ToView() => new(Id, SpeciesId, Position, Speed, VisionRange, Health);

    public bool CanEat(Animal other) => Species.Prey.Contains(other.SpeciesId);

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
