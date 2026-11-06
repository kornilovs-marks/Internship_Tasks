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

            GameSession session;

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
                        if(!File.Exists(saveFilePath))
                        {
                            Console.WriteLine($"No saved game found at {saveFilePath}");
                            continue;
                        }

                        session = await store.LoadAsync(saveFilePath);
                        break;
                    }
                    catch (Exception ex) when (ex is FileNotFoundException or InvalidDataException)
                    {
                        Console.WriteLine($"Failed to load the game: {ex.Message}");
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

                int gameCount = ReadNumber("Number of games (1-1000):", GameConstants.MinimumGames, GameConstants.MaximumGames);
                session = GameSession.CreateRandom(gameCount, width, height);
                break;
            }

            Console.Clear();
            var renderer = new GameConsoleRenderer();
            int pageIndex = 0;
            string message = "";

            var clock = Stopwatch.StartNew();
            var nextTick = clock.Elapsed + TimeSpan.FromSeconds(1);
            bool redraw = true;

            // Main loop to update the game state and handle user input
            while (true)
            {
                // Handle user input without blocking the game loop
                while(Console.KeyAvailable)
                {
                    ConsoleKey key = Console.ReadKey(intercept: true).Key;
                        
                    if(key is ConsoleKey.Q or ConsoleKey.Escape)
                        return;

                    if (key == ConsoleKey.LeftArrow && pageIndex > 0)
                    {
                        pageIndex--;
                        redraw = true;
                    }
                    if (key == ConsoleKey.RightArrow && pageIndex < session.PageCount - 1)
                    {
                        pageIndex++;
                        redraw = true;
                    }

                    if (key == ConsoleKey.S)
                    {
                        try
                        {
                            await store.SaveAsync(saveFilePath, session);
                            message = $"Saved all {session.GameCount} games to {saveFilePath}";
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
                    session.NextGeneration();
                    nextTick += TimeSpan.FromSeconds(1);
                        
                    if(nextTick <= clock.Elapsed)
                        nextTick = clock.Elapsed + TimeSpan.FromSeconds(1);
                    redraw = true;
                }

                if (redraw)
                {
                    renderer.Draw(session.Snapshot(pageIndex), message);
                    redraw = false;
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