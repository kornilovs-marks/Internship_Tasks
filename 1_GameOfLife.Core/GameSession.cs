namespace GameOfLife.Core
{
    public class GameSession
    {
        public GameOfLife[] _games;
        public int GameCount => _games.Length;

        // Inicialize constructor with games property
        public GameSession(GameOfLife[] games)
        {
            _games = games;
        }

        // Create a new GameSession with random games
        public static GameSession CreateRandom(int gameCount, int width, int height)
        {
            if (gameCount is < 1 or > GameConstants.MaximumGames)
                throw new ArgumentOutOfRangeException(nameof(gameCount));

            if (width is < GameConstants.MinimumGameFieldSize or GameConstants.MaximumWidth)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height is < GameConstants.MinimumGameFieldSize or GameConstants.MaximumHeight)
                throw new ArgumentOutOfRangeException(nameof(height));

            var games = new GameOfLife[gameCount];

            for (int i = 0; i < gameCount; i++)
            {
                games[i] = new GameOfLife(width, height);
                games[i].GridRandomizer();
            }

            return new GameSession(games);
        }

        // Advance all games to the next generation
        public void NextGeneration()
        {
            foreach (GameOfLife game in _games)
            {
                game.NextGeneration();
            }
        }

        // Get the state of a specific game by its number
        public GameState GetGameState(int gameNumber)
        {
            if (gameNumber < 1 || gameNumber > _games.Length)
                throw new ArgumentOutOfRangeException(nameof(gameNumber));

            return _games[gameNumber - 1].ToState();
        }


    }
}
