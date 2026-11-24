using Savanna.Core.Interfaces;
using Savanna.Core.Models;
using System.Text;

namespace Savanna.Console.Services;

public class ConsoleRenderer
{
    private string? _lastFrame;
    private const string EmptyCell = "··";
    private const string LionSymbol = "🦁";
    private const string AntelopeSymbol = "🦌";
    private const int MaxShownHealth = 99;    // a label must fit the 2-column cell

    // Method to render the current state of the world to the console, displaying animal positions and types
    public void Render(IWorldView world)
    {
        var shown = PickShownAnimals(world);
        var cells = new string[world.Height, world.Width];

        for (var y = 0; y < world.Height; y++)
        {
            for (var x = 0; x < world.Width; x++)
            {
                cells[y, x] = shown[y, x] is AnimalView animal ? GetAnimalSymbol(animal) : EmptyCell;
            }
        }

        // Health labels shows above the animal only if the cell above is empty
        for (var y = 1; y < world.Height; y++)
        {
            for (var x = 0; x < world.Width; x++)
            {
                if (shown[y, x] is AnimalView animal && cells[y - 1, x] == EmptyCell)
                    cells[y - 1, x] = FormatHealth(animal.Health);
            }
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

    // Decides which animal is drawn in each cell; a lion wins over an antelope on the same cell
    private static AnimalView?[,] PickShownAnimals(IWorldView world)
    {
        var shown = new AnimalView?[world.Height, world.Width];

        foreach (var animal in world.Animals)
        {
            var (x, y) = (animal.Position.x, animal.Position.y);

            if (shown[y, x] is null || animal.Kind == AnimalKind.Lion)
                shown[y, x] = animal;
        }

        return shown;
    }

    private static string GetAnimalSymbol(AnimalView animal) =>
        animal.Kind switch
        {
            AnimalKind.Lion => LionSymbol,
            AnimalKind.Antelope => AntelopeSymbol,
            _ => throw new ArgumentOutOfRangeException(nameof(animal.Kind), animal.Kind, null)
        };

    // Whole number rounded up (0.5 shows as 1, so a living animal never shows 0), padded to 2 columns
    private static string FormatHealth(double health) =>
        Math.Clamp((int)Math.Ceiling(health), 0, MaxShownHealth).ToString().PadLeft(2);
}
