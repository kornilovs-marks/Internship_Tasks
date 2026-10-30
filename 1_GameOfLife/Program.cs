using System.Diagnostics;
using System.Text;
using GameOfLife;
using GameOfLife.Core;

namespace GameOfLife_Console
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var store = new SaveGameOfLifeProgress();
            const string saveFilePath = "game_of_life_save.json";

            GameOfLife.Core.GameOfLife game;

            // First menu to choose the field size or load a saved game
            while (true)
            {
                Console.WriteLine("Conway's Game of Life");
                Console.WriteLine("0. Exit");
                Console.WriteLine("1. Small field (10x10)");
                Console.WriteLine("2. Medium field (20x20)");
                Console.WriteLine("3. Custom field size");
                Console.WriteLine("4. Open last saved game");

                int option = ReadNumber("Choose an option (0-4) :", 0, 4);

                if (option == 0)
                    return;

                if (option == 4)
                {
                    try
                    {
                        game = await store.LoadAsync(saveFilePath);
                        break;
                    }
                    catch (Exception ex) when (ex is FileNotFoundException or InvalidDataException)
                    {
                        Console.WriteLine($"Failed to load the game: {ex.Message}");
                        Console.WriteLine("Starting a new game instead.");
                        continue;
                    }
                }

                int width = option switch
                {
                    1 => 10,
                    2 => 20,
                    _ => ReadNumber("Enter the width of the field (5-35):", GameConstants.MinimumGameFieldSize, GameConstants.MaximumWidth)
                };

                int height = option switch
                {
                    1 => 10,
                    2 => 20,
                    _ => ReadNumber("Enter the height of the field (5-20):", GameConstants.MinimumGameFieldSize, GameConstants.MaximumHeight)
                };

                game = new GameOfLife.Core.GameOfLife(width, height);
                game.GridRandomizer();
                break;
            }

            Console.Clear();
            string message = "";

            var clock = Stopwatch.StartNew();
            var nextTick = clock.Elapsed + TimeSpan.FromSeconds(1);
            bool redraw = true;

            // Main loop to update the game state and handle user input
            while (true)
            {
                if (redraw)
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine($"Conway's Game of Life |");
                    Console.WriteLine($"Iteration: {game.Iteration} | Living cells: {game.LivingCells}");
                    Console.WriteLine("S = save | Q = quit");
                    // To show the message that the game is saved
                    Console.WriteLine(message.PadRight(70));

                    for (int row = 0; row < game.Height; row++)
                    {
                        for (int column = 0; column < game.Width; column++)
                        {
                            Console.Write(game.IsAlive(row, column) ? "██" : "  ");
                        }
                        Console.WriteLine();
                    }
                    redraw = false;
                }

                // Handle user input without blocking the game loop
                while(Console.KeyAvailable)
                {
                    ConsoleKey key = Console.ReadKey(intercept: true).Key;
                        
                    if(key is ConsoleKey.Q or ConsoleKey.Escape)
                        return;

                    if (key == ConsoleKey.S)
                    {
                        try
                        {
                            await store.SaveAsync(saveFilePath, game);
                            message = $"Saved to {saveFilePath}.";
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            message = $"Save failed: {ex.Message}";
                        }
                        redraw = true;
                    }
                }

                if(clock.Elapsed >= nextTick)
                {
                    game.NextGeneration();
                    nextTick += TimeSpan.FromSeconds(1);
                        
                    if(nextTick <= clock.Elapsed)
                        nextTick = clock.Elapsed + TimeSpan.FromSeconds(1);
                    redraw = true;
                }
            }

            // Local function to read a number from the console with validation
            static int ReadNumber(string prompt, int mimimum, int maximum)
            {
                while(true)
                {
                    Console.WriteLine(prompt);
                    if (int.TryParse(Console.ReadLine(), out int number) && number >= mimimum && number <= maximum)
                        return number;
                    Console.WriteLine($"Enter a number between {mimimum} and {maximum}.");
                }
            }
        }
    }
}