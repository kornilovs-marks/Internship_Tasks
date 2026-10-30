using System.Text.Json;

namespace GameOfLife.Core
{
    public class SaveGameOfLifeProgress
    {
        // Save the game state to a file. Defaults to Core root directory
        public async Task SaveAsync(string path, GameOfLife game)
        {
            string fullPath = Path.GetFullPath(path);
            string temporaryPath = fullPath + ".tmp";

            try
            {
                await using (var stream = File.Create(temporaryPath))
                    await JsonSerializer.SerializeAsync(stream, game.ToState());

                File.Move(temporaryPath, fullPath, overwrite: true);
            }
            finally
            {
                if(File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }

        // Load the game state from a file and return a new GameOfLife instance
        public async Task<GameOfLife> LoadAsync(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            await using var stream = File.OpenRead(path);
            var state = await JsonSerializer.DeserializeAsync<GameState>(stream) ??
                throw new InvalidDataException("The file does not contain a valid game state.");

            return GameOfLife.FromState(state);
        }
    }
}
