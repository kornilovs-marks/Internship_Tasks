namespace Savanna.Console.Services;

public enum ConsoleCommand
{
    AddAntelope,
    AddLion,
    Quit
}

// Service class that handles reading user input from the console and translating it into game commands
public class ConsoleInputService
{
    public IEnumerable<ConsoleCommand> ReadAvailable()
    {
        while (System.Console.KeyAvailable)
        {
            var key = System.Console.ReadKey(intercept: true).Key;

            switch (key)
            {
                case ConsoleKey.A:
                    yield return ConsoleCommand.AddAntelope;
                    break;
                case ConsoleKey.L:
                    yield return ConsoleCommand.AddLion;
                    break;
                case ConsoleKey.Q:
                    yield return ConsoleCommand.Quit;
                    break;
            }
        }
    }
}
