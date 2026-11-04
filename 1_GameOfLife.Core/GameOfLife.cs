namespace GameOfLife.Core
{
    public class GameOfLife
    {
        private bool[,] _cells;
        public int Width { get; }
        public int Height { get; }
        public long Iteration { get; set; }
        public int LivingCells { get; set; }

        // Initialize the game with a given width and height
        public GameOfLife(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new bool[height, width];
        }

        // Randomize the field with alive and dead cells
        public void GridRandomizer()
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    _cells[row, column] = Random.Shared.Next(GameConstants.InitialCellChoices) == GameConstants.InitialAliveOutcome;
                }
            }
        }

        // Initialize GameOfLife overloading with width, height and iteration
        public GameOfLife(int width, int height, long iteration)
        {
            Width = width;
            Height = height;
            Iteration = iteration;
            _cells = new bool[height, width];
        }

        // Lamda expression to check if a cell is alive
        public bool IsAlive(int row, int column) => _cells[row, column];

        // Update the field to the next generation
        public void NextGeneration()
        {
            var next = new bool[Height, Width];
            int living = 0;

            for (int row = 0; row < Height; row++)
                for (int column = 0; column < Width; column++)
                {
                    int neighbors = CountLivingNeighbors(row, column);

                    next[row, column] = neighbors == GameConstants.NeighborsToBecomeAlive || (_cells[row, column] && neighbors == GameConstants.NeighborsToStayAlive); ;

                    if (next[row, column])
                        living++;
                }

            _cells = next;
            LivingCells = living;
            Iteration++;
        }

        // Create a GameState object from the current state of the game
        public GameState ToState()
        {
            var cells = new bool[Height * Width];
            for(int row = 0; row < Height; row++)
            {
                for(int column = 0; column < Width; column++)
                {
                    cells[row * Width + column] = _cells[row, column];
                }
            }
            return new GameState(Width, Height, Iteration, cells);
        }

        // Create a GameOfLife object from a given GameState object
        public static GameOfLife FromState(GameState state)
        {
            if (state is null || state.Width is < 1 or > GameConstants.MaximumWidth || state.Height is < 1 or > GameConstants.MaximumHeight
                || state.Iteration < 0 || state.Cells is null || state.Cells.Length != state.Width * state.Height)
                throw new ArgumentException("Invalid game state.");

            var game = new GameOfLife(state.Width, state.Height, state.Iteration);
            for (int row = 0; row < game.Height; row++)
            {
                for (int column = 0; column < game.Width; column++)
                {
                    bool alive = state.Cells[row * game.Width + column];
                    game._cells[row, column] = alive;

                    if (alive) 
                        game.LivingCells++;
                }
            }
            return game;
        }

        // Count the number of living neighbors for a given cell
        private int CountLivingNeighbors(int row, int column)
        {
            int count = 0;

            for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
            {
                for (int columnOffset = -1; columnOffset <= 1; columnOffset++)
                {
                    if (rowOffset == 0 && columnOffset == 0) continue;

                    int neighborRow = row + rowOffset;
                    int neighborColumn = column + columnOffset;

                    if (neighborRow >= 0 && neighborRow < Height && neighborColumn >= 0 && neighborColumn < Width && _cells[neighborRow, neighborColumn])
                        count++;
                }
            }

            return count;
        }
    }
}