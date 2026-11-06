namespace GameOfLife.Core
{
    public record GameSessionState(int Version, GameState[] Games);
}
