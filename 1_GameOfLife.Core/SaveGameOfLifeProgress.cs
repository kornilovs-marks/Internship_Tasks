using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameOfLife.Core
{
    public class SaveGameOfLifeProgress
    {
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
