using Savanna.Core.Behaviors;
using Savanna.Core.Configuration;
using Savanna.Core.Interfaces;
using Savanna.Core.Models;

namespace Savanna.Core.Factories;

public class AntelopeFactory(GameOptions options, Random random) : AnimalFactoryBase
{
    public override AnimalKind Kind => AnimalKind.Antelope;

    public override Animal Create(Position position) =>
        new Antelope(position, options.AntelopeSpeed, options.AntelopeVision, new AntelopeBehavior(random));
}
