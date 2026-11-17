using Savanna.Core.Interfaces;
using Savanna.Core.Models;
using System.Text;

namespace Savanna.Console.Services;

public class ConsoleRenderer
{
    private string? _lastFrame;
    private const string Empty = "··";
    private const string LionSymbol = "🦁";
    private const string AntelopeSymbol = "🦌";

    // Method to render the current state of the world to the console, displaying animal positions and types
    public void Render(IWorldView world)
    {
        var cells = new string[world.Height, world.Width];
        for(var y = 0; y < world.Height; y++)
        {
            for (var x = 0; x < world.Width; x++)
            {
                cells[y, x] = Empty;
            }
        }

        foreach (var animal in world.Animals)
        {
            if (animal.Kind == AnimalKind.Lion || cells[animal.Position.y, animal.Position.x] == Empty)
                cells[animal.Position.y, animal.Position.x] = animal.Kind == AnimalKind.Lion ? LionSymbol : AntelopeSymbol;
        }

        var output = new StringBuilder();
        output.AppendLine($"Savanna | A: add {AntelopeSymbol}  L: add {LionSymbol}  Q: quit");
        var counts = $"{AntelopeSymbol}: {world.Animals.Count(animal => animal.Kind == AnimalKind.Antelope)}  " +
             $"{LionSymbol}: {world.Animals.Count(animal => animal.Kind == AnimalKind.Lion)}";
        output.AppendLine(counts.PadRight(40));

        for (var y = 0; y < world.Height; y++)
        {
            for (var x = 0; x < world.Width; x++)
                output.Append(cells[y, x]);
            output.AppendLine();
        }

        var frame = output.ToString();

        if (frame == _lastFrame)
            return;

        _lastFrame = frame;

        System.Console.SetCursorPosition(0, 0);
        System.Console.Write(frame);
    }
}