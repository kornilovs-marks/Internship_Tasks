using Savanna.AnimalsBase.Factories;
using Savanna.AnimalsBase.Interfaces;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Behaviors;
using Savanna.Core.Configuration;

namespace Savanna.Core.Factories;

public class LionFactory(GameOptions options, Random random) : AnimalFactoryBase
{
    public static readonly SpeciesId Id = new("Lion");

    public override SpeciesDescriptor Species { get; } = new(
        Id, "Lion", "🦁", 'L',
        options.LionSpeed, options.LionVision, options.LionHealth,
        new HashSet<SpeciesId> { AntelopeFactory.Id });

    public override IAnimalBehavior CreateBehavior() => new LionBehavior(random);
}
