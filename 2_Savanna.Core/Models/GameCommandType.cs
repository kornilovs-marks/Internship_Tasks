namespace Savanna.AnimalsBase.Models;

public enum GameCommandType
{
    AddAnimal = 1,
    Quit = 2
}

public readonly record struct GameCommand(GameCommandType Type, SpeciesId? Species = null);