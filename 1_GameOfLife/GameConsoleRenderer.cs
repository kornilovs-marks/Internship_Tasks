using GameOfLife.Core;

namespace GameOfLife
{
    public class GameConsoleRenderer
    {
        private const int PreviewWidth = 14;
        private const int PreviewHeight = 6;
        private const int CardWidth = 18;
        private const int CardHeight = PreviewHeight + 2;

        public void Draw(GameSessionSnapshot snapshot, string message)
        {
            Console.Clear();
            Console.WriteLine($"Conway's Game of Life | Iteration: {snapshot.Iteration}");
            Console.WriteLine($"Live games: {snapshot.LiveGames}/{snapshot.TotalGames} | Living cells in total: {snapshot.TotalLivingCells}");
            Console.WriteLine($"Page {snapshot.PageIndex + 1}/{snapshot.PageCount} | Left/Right = page | S = save all | Q = quit");
            Console.WriteLine(message.Length > 72 ? message[..72] : message);

            // Four cards across and two rows on a typical 80x24 terminal. A smaller
            // terminal shows only the cards that fit so totals remain visible.
            int columns = Math.Clamp((Console.WindowWidth - 1) / CardWidth, 1, 4);
            int rows = Math.Max(0, (Console.WindowHeight - 5) / CardHeight);
            int visibleCount = Math.Min(snapshot.VisibleGames.Length, columns * rows);

            for (int first = 0; first < visibleCount; first += columns)
            {
                var cards = snapshot.VisibleGames.Skip(first).Take(Math.Min(columns, visibleCount - first)).ToArray();
                foreach (var game in cards)
                    Console.Write($"#{game.Id} living:{game.LivingCells}".PadRight(CardWidth));
                Console.WriteLine();

                for (int row = 0; row < PreviewHeight; row++)
                {
                    foreach (var game in cards)
                    {
                        Console.Write('│');
                        for (int column = 0; column < PreviewWidth; column++)
                        {
                            bool inField = row < game.State.Height && column < game.State.Width;
                            Console.Write(!inField ? ' ' :
                                game.State.Cells[row * game.State.Width + column] ? '█' : '·');
                        }
                        Console.Write("│  ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            if (visibleCount < snapshot.VisibleGames.Length)
                Console.Write("Enlarge the terminal to show all eight games on this page.");
            else
                Console.Write("Each preview shows the upper-left 14 x 6 cells.");
        }
    }
}
