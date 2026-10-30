namespace GameOfLife.Core
{
    public record GameState(int Width, int Height, long Iteration, bool[] Cells);
}
