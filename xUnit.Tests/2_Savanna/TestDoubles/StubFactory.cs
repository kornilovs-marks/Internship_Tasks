using Savanna.AnimalsBase.Factories;
using Savanna.AnimalsBase.Interfaces;
using Savanna.AnimalsBase.Models;

namespace xUnit.Tests.Savanna.TestDoubles;

public class StubFactory(SpeciesDescriptor species) : AnimalFactoryBase
{
    public override SpeciesDescriptor Species => species;

    public override IAnimalBehavior CreateBehavior() => StubBehavior.StandStill();
}
