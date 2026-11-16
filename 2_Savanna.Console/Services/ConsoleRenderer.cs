using Savanna.Core.Interfaces;
using Savanna.Core.Models;
using System.Text;

namespace Savanna.Console.Services;

public class ConsoleRenderer
{
    private string? _lastFrame;

    // Method to render the current state of the world to the console, displaying animal positions and types
    public void Render(IWorldView world)
    {
        var cells = new char[world.Height, world.Width];
        for(var y = 0; y < world.Height; y++)
        {
            for (var x = 0; x < world.Width; x++)
            {
                cells[y, x] = '.';
            }
        }

        foreach (var animal in world.Animals)
        {
            if (animal.Kind == AnimalKind.Lion || cells[animal.Position.y, animal.Position.x] == '.')
                cells[animal.Position.y, animal.Position.x] = animal.Kind == AnimalKind.Lion ? 'L' : 'A';
        }

        var output = new StringBuilder();
        output.AppendLine("Savanna | A: add antelope  L: add lion  Q: quit");
        output.AppendLine($"Antelopes: {world.Animals.Count(animal => animal.Kind == AnimalKind.Antelope)}  " +
                          $"Lions: {world.Animals.Count(animal => animal.Kind == AnimalKind.Lion)}");

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

        System.Console.Clear();
        System.Console.Write(frame);
    }
}