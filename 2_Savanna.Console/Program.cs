using Microsoft.Extensions.DependencyInjection;
using Savanna.Console.Services;
using Savanna.Core.Configuration;
using Savanna.Core.Factories;
using Savanna.Core.Interfaces;
using Savanna.Core.Models;
using Savanna.Core.Services;
using System.Diagnostics;

if (System.Console.IsInputRedirected)
{
    System.Console.Error.WriteLine("Savanna requires an interactive terminal for A, L, and Q input.");
    return 1;
}

var options = new GameOptions();
options.Validate();

var services = new ServiceCollection();
services.AddSingleton(options);
services.AddSingleton<Random>();
services.AddSingleton<AnimalFactoryBase, AntelopeFactory>();
services.AddSingleton<AnimalFactoryBase, LionFactory>();
services.AddSingleton<IAnimalFactory, AnimalFactory>();
services.AddSingleton<GameWorld>();
services.AddSingleton<GameEngine>();
services.AddSingleton<ConsoleInputService>();
services.AddSingleton<ConsoleRenderer>();

using var provider = services.BuildServiceProvider(validateScopes: true);

var engine = provider.GetRequiredService<GameEngine>();
var renderer = provider.GetRequiredService<ConsoleRenderer>();
var input = provider.GetRequiredService<ConsoleInputService>();

var tick = TimeSpan.FromMilliseconds(options.TickMilliseconds);
var clock = Stopwatch.StartNew();
var previous = clock.Elapsed;
var accumulated = TimeSpan.Zero;
var running = true;

try
{
    System.Console.OutputEncoding = System.Text.Encoding.UTF8;
    System.Console.CursorVisible = false;
    System.Console.Clear();
    renderer.Render(engine.World.Snapshot());

    while (running)
    {
        foreach(var command in input.ReadAvailable())
        {
            switch (command)
            {
                case ConsoleCommand.AddAntelope:
                    engine.TryAddAnimal(AnimalKind.Antelope);
                    renderer.Render(engine.World.Snapshot());
                    break;
                case ConsoleCommand.AddLion:
                    engine.TryAddAnimal(AnimalKind.Lion);
                    renderer.Render(engine.World.Snapshot());
                    break;
                case ConsoleCommand.Quit:
                    running = false;
                    break;
            }
        }

        if (!running)
            break;

        var now = clock.Elapsed;
        accumulated += now - previous;
        previous = now;

        var updated = false;
        while(accumulated >= tick)
        {
            engine.Tick(tick);
            accumulated -= tick;
            updated = true;
        }

        if(updated)
            renderer.Render(engine.World.Snapshot());
    }
}
finally
{
    System.Console.CursorVisible = true;
    System.Console.WriteLine("Game over.");
}
return 0;