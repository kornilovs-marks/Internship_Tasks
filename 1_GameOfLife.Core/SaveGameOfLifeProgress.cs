using System.Text.Json;

namespace GameOfLife.Core
{
    public class SaveGameOfLifeProgress
    {
        // Save the game session to a file in JSON format. Defaults to Core root directory
        public async Task SaveAsync(string path, GameSession session)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentNullException.ThrowIfNull(session);

            GameSessionState state = session.ToState();
            string fullPath = Path.GetFullPath(path);
            string temporaryPath = fullPath + ".tmp";

            try
            {
                await using (var stream = File.Create(temporaryPath))
                    await JsonSerializer.SerializeAsync(stream, state);

                File.Move(temporaryPath, fullPath, overwrite: true);
            }
            finally
            {
                if(File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }

        // Load the game session from a file and returns GameSession object
        public async Task<GameSession> LoadAsync(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            await using var stream = File.OpenRead(path);
            var state = await JsonSerializer.DeserializeAsync<GameSessionState>(stream) ??
                throw new InvalidDataException("The file does not contain a valid game state.");

            return GameSession.FromState(state);
        }
    }
}
