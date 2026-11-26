using Savanna.Core.Models;
using Savanna.AnimalsBase.Models;

namespace Savanna.Core.Interfaces;

public interface IAnimalFactory
{
    Animal Create(SpeciesId species, Position position);
}
