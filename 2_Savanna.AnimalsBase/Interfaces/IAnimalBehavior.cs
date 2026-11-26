using Savanna.AnimalsBase.Interfaces;
using Savanna.AnimalsBase.Models;

namespace Savanna.AnimalsBase.Interfaces;

public interface IAnimalBehavior
{
    AnimalActions ChooseAction(AnimalView animal, IWorldView world);
}
