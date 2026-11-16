using Savanna.Core.Models;

namespace Savanna.Core.Factories;

public abstract class AnimalFactoryBase
{
    public abstract AnimalKind Kind { get; }
    public abstract Animal Create(Position position);
}
