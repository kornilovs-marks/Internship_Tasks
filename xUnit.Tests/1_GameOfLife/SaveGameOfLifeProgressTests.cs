using FluentAssertions;
using GameOfLife.Core;
using System.Text.Json;

namespace xUnit.Tests._1_GameOfLife
{
    [Trait("Category", "Persistence")]
    public class SaveGameOfLifeProgressTests: IDisposable
    {
        private readonly string _testDirectory;
        private readonly SaveGameOfLifeProgress _saveService;

        public SaveGameOfLifeProgressTests()
        {
            _testDirectory = Path.Combine(
                Path.GetTempPath(),
                "GameOfLifeTests",
                Guid.NewGuid().ToString());

            Directory.CreateDirectory(_testDirectory);

            _saveService = new SaveGameOfLifeProgress();
        }

        [Fact]
        public async Task SaveAsync_ShouldCreateFile()
        {
            // Arrange
            string path = GetTestPath();
            GameSession session = CreateSession();

            // Act
            await _saveService.SaveAsync(path, session);

            // Assert
            File.Exists(path).Should().BeTrue();
        }

        [Fact]
        public async Task SaveAsync_ShouldCreateValidJson()
        {
            // Arrange
            string path = GetTestPath();
            GameSession session = CreateSession();

            // Act
            await _saveService.SaveAsync(path, session);

            // Assert
            string json = await File.ReadAllTextAsync(path);

            Action parseJson = () => JsonDocument.Parse(json);

            parseJson.Should().NotThrow();
        }

        [Fact]
        public async Task SaveAsync_ShouldCreateDeserializableGameSessionState()
        {
            // Arrange
            string path = GetTestPath();
            GameSession session = CreateSession();

            // Act
            await _saveService.SaveAsync(path, session);

            // Assert
            string json = await File.ReadAllTextAsync(path);

            GameSessionState? state = JsonSerializer.Deserialize<GameSessionState>(json);

            state.Should().NotBeNull();
        }

        [Fact]
        public async Task LoadAsync_ShouldReturnGameSessionFromSavedFile()
        {
            // Arrange
            string path = GetTestPath();
            GameSession originalSession = CreateSession();

            await _saveService.SaveAsync(path, originalSession);

            // Act
            GameSession loadedSession = await _saveService.LoadAsync(path);

            // Assert
            loadedSession.Should().NotBeNull();
            loadedSession.Games.Should().HaveCount(1);

            var originalGame = originalSession.Games[0];
            var loadedGame = loadedSession.Games[0];

            loadedGame.Width.Should().Be(originalGame.Width);
            loadedGame.Height.Should().Be(originalGame.Height);
            loadedGame.Iteration.Should().Be(originalGame.Iteration);
            loadedGame.LivingCells.Should().Be(originalGame.LivingCells);
        }

        [Fact]
        public async Task LoadAsync_WhenFileDoesNotExist_ShouldThrowFileNotFoundException()
        {
            // Arrange / Act / Assert as one-liner
            await FluentActions.Invoking(() => _saveService.LoadAsync(GetTestPath())).Should().ThrowAsync<FileNotFoundException>();
        }

        [Fact]
        public async Task LoadAsync_WhenFileIsEmpty_ShouldThrowInvalidDataException()
        {
            // Arrange
            string path = GetTestPath();
            await File.WriteAllTextAsync(path, string.Empty);

            // Act
            Func<Task> act = () => _saveService.LoadAsync(path);

            // Assert
            await act.Should().ThrowAsync<JsonException>();
        }

        [Fact]
        public async Task LoadAsync_WhenJsonIsMalformed_ShouldThrowJsonException()
        {
            // Arrange
            string path = GetTestPath();
            await File.WriteAllTextAsync(path, "{ invalid json }");

            // Act
            Func<Task> act = () => _saveService.LoadAsync(path);

            // Assert
            await act.Should().ThrowAsync<JsonException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadAsync_WhenPathIsInvalid_ShouldThrowArgumentException(string? path)
        {
            // Arrange / Act / Assert as one-liner
            await FluentActions.Invoking(() => _saveService.LoadAsync(path)).Should().ThrowAsync<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SaveAsync_WhenPathIsInvalid_ShouldThrowArgumentException(string? path)
        {
            // Arrange / Act / Assert as one-liner
            await FluentActions.Invoking(() => _saveService.SaveAsync(path!, CreateSession())).Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task SaveAsync_WhenSessionIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange / Act / Assert as one-liner
            await FluentActions.Invoking(() => _saveService.SaveAsync(GetTestPath(), null!)).Should().ThrowAsync<ArgumentNullException>().WithParameterName("session");
        }

        [Fact]
        public async Task SaveAsync_WhenFileAlreadyExists_ShouldOverwriteFile()
        {
            // Arrange
            string path = GetTestPath();

            await File.WriteAllTextAsync(path, "old invalid content");

            GameSession session = CreateSession();

            // Act
            await _saveService.SaveAsync(path, session);

            // Assert
            string content = await File.ReadAllTextAsync(path);

            content.Should().NotBe("old invalid content");

            Action parseJson = () => JsonDocument.Parse(content);
            parseJson.Should().NotThrow();
        }

        [Fact]
        public async Task SaveAsync_AfterSuccessfulSave_ShouldNotLeaveTemporaryFile()
        {
            // Arrange
            string path = GetTestPath();
            string temporaryPath = path + ".tmp";
            GameSession session = CreateSession();

            // Act
            await _saveService.SaveAsync(path, session);

            // Assert
            File.Exists(path).Should().BeTrue();
            File.Exists(temporaryPath).Should().BeFalse();
        }

        // Incialize a new test path for each test to avoid conflicts
        private string GetTestPath()
        {
            return Path.Combine(_testDirectory, "game-state.json");
        }

        // Create a simple GameSession for testing purposes
        private static GameSession CreateSession()
        {

            var gameOfLife = new GameOfLife.Core.GameOfLife(5, 5);
            var game = new[]
            {
                gameOfLife
            };

            return new GameSession(game);
        }

        // Clean up the test directory after tests are done
        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
        }
    }
}
