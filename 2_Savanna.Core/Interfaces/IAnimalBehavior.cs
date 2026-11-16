using Savanna.Core.Models;

namespace Savanna.Core.Interfaces;

public interface IAnimalBehavior
{
    AnimalKind Kind { get; }
    AnimalActions ChooseAction(AnimalView animal, IWorldView world);
}
