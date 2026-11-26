using Savanna.AnimalsBase.Models;

namespace xUnit.Tests.Savanna.TestDoubles;

public static class TestSpecies
{
    public const double DefaultSpeed = 1;
    public const int DefaultVision = 9;
    public const double DefaultHealth = 20;

    public static readonly SpeciesId PreyId = new("TestPrey");
    public static readonly SpeciesId PredatorId = new("TestPredator");

    public static SpeciesDescriptor Prey(double speed = DefaultSpeed, int vision = DefaultVision, double health = DefaultHealth) =>
        new(PreyId, "Prey", "pp", 'P', speed, vision, health, new HashSet<SpeciesId>());

    public static SpeciesDescriptor Predator(double speed = DefaultSpeed, int vision = DefaultVision, double health = DefaultHealth) =>
        new(PredatorId, "Predator", "PP", 'R', speed, vision, health, new HashSet<SpeciesId> { PreyId });

    public static SpeciesDescriptor For(Role role, double speed = DefaultSpeed, int vision = DefaultVision, double health = DefaultHealth) =>
        role == Role.Predator ? Predator(speed, vision, health) : Prey(speed, vision, health);

    public static SpeciesId IdFor(Role role) => role == Role.Predator ? PredatorId : PreyId;
}
