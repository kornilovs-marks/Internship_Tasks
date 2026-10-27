using System.Text;

namespace GameOfLife
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Conway's Game of Life");
            Console.WriteLine("1. Small field (10x10)");
            Console.WriteLine("2. Medium field (20x20)");
            Console.WriteLine("3. Custom field size");

            int option = ReadNumber("Choose an option (1-3) :", 1, 3);

            int width = option switch
            {
                1 => 10,
                2 => 20,
                _ => ReadNumber("Enter the width of the field (5-35):", 5, 35)
            };

            int height = option switch
            {
                1 => 10,
                2 => 20,
                _ => ReadNumber("Enter the height of the field (5-20):", 5, 20)
            };

            var game = new GameOfLife(width, height);
            game.GridRandomizer();

            while(true)
            {
                Console.Clear();
                Console.WriteLine("Conway's Game of Life");
                for(int row = 0; row < game.Height; row++)
                {
                    for(int column = 0; column < game.Width; column++)
                    {
                        Console.Write(game.IsAlive(row, column) ? "██" : "  ");
                    }
                    Console.WriteLine();
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
                game.NextGeneration();
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