using Savanna.Console.Models;
using Savanna.Core.Models;

namespace Savanna.Console.Services;

// Service class that handles reading user input from the console and translating it into game commands
public class ConsoleInputService
{
    public IEnumerable<GameCommand> ReadAvailable()
    {
        while (System.Console.KeyAvailable)
        {
            var key = System.Console.ReadKey(intercept: true).Key;

            switch (key)
            {
                case ConsoleKey.A:
                    yield return new GameCommand(GameCommandType.AddAnimal, AnimalKind.Antelope);
                    break;
                case ConsoleKey.L:
                    yield return new GameCommand(GameCommandType.AddAnimal, AnimalKind.Lion);
                    break;
                case ConsoleKey.Q:
                    yield return new GameCommand(GameCommandType.Quit);
                    break;
            }
        }
    }
}
