using Savanna.Core.Models;

namespace Savanna.Core.Interfaces;

public interface IAnimalBehavior
{
    AnimalActions ChooseAction(AnimalView animal, IWorldView world);
}
