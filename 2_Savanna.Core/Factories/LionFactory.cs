using Savanna.Core.Behaviors;
using Savanna.Core.Configuration;
using Savanna.Core.Interfaces;
using Savanna.Core.Models;

namespace Savanna.Core.Factories;

public class LionFactory(GameOptions options, Random random) : AnimalFactoryBase
{
    public override AnimalKind Kind => AnimalKind.Lion;

    public override Animal Create(Position position) =>
        new Lion(position, options.LionSpeed, options.LionVision, new LionBehavior(random));
}
