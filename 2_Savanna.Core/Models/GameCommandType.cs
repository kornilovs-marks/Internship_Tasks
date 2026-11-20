namespace Savanna.Core.Models;

public enum GameCommandType
{
    AddAnimal = 1,
    Quit = 2
}

public readonly record struct GameCommand(GameCommandType Type, AnimalKind? Kind = null);