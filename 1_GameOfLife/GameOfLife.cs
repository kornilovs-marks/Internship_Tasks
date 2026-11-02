namespace GameOfLife
{
    public class GameOfLife
    {
        private bool[,] _cells;
        public int Width { get; }
        public int Height { get; }


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

        // Lamda expression to check if a cell is alive
        public bool IsAlive(int row, int column) => _cells[row, column];

        // Update the field to the next generation
        public void NextGeneration()
        {
            var next = new bool[Height, Width];

            for (int row = 0; row < Height; row++)
                for (int column = 0; column < Width; column++)
                {
                    int neighbors = CountLivingNeighbors(row, column);
                    next[row, column] = neighbors == GameConstants.NeighborsToBecomeAlive || (_cells[row, column] && neighbors == GameConstants.NeighborsToStayAlive);
                }

            _cells = next;
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