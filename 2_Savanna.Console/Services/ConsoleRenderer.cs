using Savanna.AnimalsBase.Interfaces;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Services;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Savanna.Console.Services;

public class ConsoleRenderer(SpeciesRegistry species)
{
    private string? _lastFrame;
    private const string EmptyCell = "··";
    private const string LionSymbol = "🦁";
    private const string AntelopeSymbol = "🦌";
    private const string UnknownSymbol = "??";
    private const int CellWidth = 2;
    private const int LargestHpLabel = 99;
    private const int StatusLineWidth = 40;
    private readonly IReadOnlyList<SpeciesDescriptor> _species = species.All;
    private readonly IReadOnlyDictionary<SpeciesId, string> _symbols =
        species.All.ToDictionary(static descriptor => descriptor.Id, static descriptor => descriptor.Symbol);


    // Method to render the current state of the world to the console, displaying animal positions, health and types
    public void Render(IWorldView world)
    {
        var shown = PickShownAnimals(world);
        var cells = new string[world.Height, world.Width];

        for (var y = 0; y < world.Height; y++)
        {
            for (var x = 0; x < world.Width; x++)
            {
                cells[y, x] = shown[y, x] is AnimalView animal ? _symbols.GetValueOrDefault(animal.Species, UnknownSymbol) : EmptyCell;
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
        var keys = string.Join("  ", _species.Select(static descriptor => $"{char.ToUpperInvariant(descriptor.Key)}: add {descriptor.Symbol}"));
        output.AppendLine($"Savanna | {keys}  Q: quit".PadRight(StatusLineWidth));
        var counts = string.Join("  ", _species.Select(descriptor =>
            $"{descriptor.Symbol}: {world.Animals.Count(animal => animal.Species == descriptor.Id)}"));
        output.AppendLine(counts.PadRight(StatusLineWidth));

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

            if (shown[y, x] is not AnimalView current || world.CanEat(animal.Species, current.Species))
                shown[y, x] = animal;
        }

        return shown;
    }

    // Whole number rounded up so at most only 2 char are visible on the board, padded to 2 columns
    private static string FormatHealth(double health)
    {
        var label = Math.Min((int)Math.Ceiling(health), LargestHpLabel);
        return $"{label,CellWidth}";
    }
}
