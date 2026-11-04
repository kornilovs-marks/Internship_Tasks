namespace GameOfLife.Core
{
    public sealed record GamePreview(int Id, GameState State, int LivingCells);
    public sealed record GameSessionSnapshot(
        long Iteration,
        int TotalGames,
        int LiveGames,
        long TotalLivingCells,
        int PageIndex,
        int PageCount,
        GamePreview[] VisibleGames
    );
}
