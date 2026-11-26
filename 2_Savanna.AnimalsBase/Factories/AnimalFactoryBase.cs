using Savanna.AnimalsBase.Models;
using Savanna.AnimalsBase.Interfaces;

namespace Savanna.AnimalsBase.Factories;

public abstract class AnimalFactoryBase
{
    public abstract SpeciesDescriptor Species { get; }
    public abstract IAnimalBehavior CreateBehavior();
}
