using Savanna.AnimalsBase.Factories;
using Savanna.AnimalsBase.Models;
using Savanna.AnimalsBase.Interfaces;
using Savanna.Core.Behaviors;
using Savanna.Core.Configuration;

namespace Savanna.Core.Factories;

public class AntelopeFactory(GameOptions options, Random random) : AnimalFactoryBase
{
    public static readonly SpeciesId Id = new("Antelope");

    public override SpeciesDescriptor Species { get; } = new(
        Id, "Antelope", "🦌", 'A',
        options.AntelopeSpeed, options.AntelopeVision, options.AntelopeHealth,
        new HashSet<SpeciesId>());

    public override IAnimalBehavior CreateBehavior() => new AntelopeBehavior(random);
}
