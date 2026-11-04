namespace GameOfLife.Core
{
    public class GameSession
    {
        private GameOfLife[] _games;
        private object _gate = new();
        public int GameCount => _games.Length;

        public int PageCount => (_games.Length + GameConstants.GamesPerPage - 1) / GameConstants.GamesPerPage;

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

            var games = new GameOfLife[gameCount];

            for (int i = 0; i < gameCount; i++)
            {
                games[i] = new GameOfLife(width, height);
                games[i].GridRandomizer();
            }

            return new GameSession(games);
        }

        // Update all games to the next generation
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

        public GameSessionSnapshot Snapshot(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= PageCount)
                throw new ArgumentOutOfRangeException(nameof(pageIndex));

            lock (_gate)
            {
                int first = pageIndex * GameConstants.GamesPerPage;
                int last = Math.Min(first + GameConstants.GamesPerPage, _games.Length);
                var previews = new GamePreview[last - first];

                for (int index = first; index < last; index++)
                {
                    previews[index - first] = new GamePreview(index + 1, _games[index].ToState(), _games[index].LivingCells);
                }

                int liveGames = 0;
                long totalLivingCells = 0;
                foreach (var game in _games)
                {
                    if (game.LivingCells > 0) liveGames++;
                    totalLivingCells += game.LivingCells;
                }

                return new GameSessionSnapshot(_games[0].Iteration, _games.Length,
                    liveGames, totalLivingCells, pageIndex, PageCount, previews);
            }
        }

        // Convert the current game session to a state object for saving
        public GameSessionState ToState()
        {
            lock (_gate)
                return new GameSessionState(1, _games.Select(game => game.ToState()).ToArray());
        }

        // Create a all games session from a saved state
        public static GameSession FromState(GameSessionState state)
        {
            if (state is null || state.Version != 1 || state.Games is null ||
                state.Games.Length is < 1 or > GameConstants.MaximumGames)
                throw new InvalidDataException("Invalid or unsupported game session.");

            var games = new GameOfLife[state.Games.Length];
            for (int i = 0; i < games.Length; i++)
                games[i] = GameOfLife.FromState(state.Games[i]);

            // All games share one clock, so a save cannot mix different iterations.
            if (games.Any(game => game.Iteration != games[0].Iteration))
                throw new InvalidDataException("Games in the save have different iterations.");

            return new GameSession(games);
        }

    }
}
