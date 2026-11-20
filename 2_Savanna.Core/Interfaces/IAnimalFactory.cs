using Savanna.Core.Models;

namespace Savanna.Core.Interfaces;

public interface IAnimalFactory
{
    Animal Create(AnimalKind kind, Position position);
}
