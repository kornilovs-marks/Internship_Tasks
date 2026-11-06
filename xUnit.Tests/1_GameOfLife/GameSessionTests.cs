using FluentAssertions;
using GameOfLife.Core;

namespace xUnit.Tests._1_GameOfLife
{
    [Trait("Category", "GameSession")]
    public class GameSessionTests
    {
        [Fact]
        public void Snapshot_ShouldReturnSnapshotForRequestedPage()
        {
            // Arrange
            GameSession session = CreateSession();

            // Act
            GameSessionSnapshot snapshot = session.Snapshot(0);

            // Assert
            snapshot.Should().NotBeNull();
            snapshot.VisibleGames.Should().HaveCount(1);
        }

        [Fact]
        public void Snapshot_ShouldContainStateOfSessionGame()
        {
            // Arrange
            GameSession session = CreateSession();
            GameState expectedGameState = session.Games[0].ToState();

            // Act
            GameSessionSnapshot snapshot = session.Snapshot(0);

            // Assert
            snapshot.VisibleGames.Should().HaveCount(1);
            snapshot.VisibleGames[0].Id.Should().Be(1);
            snapshot.VisibleGames[0].State.Should().BeEquivalentTo(expectedGameState);
            snapshot.VisibleGames[0].LivingCells.Should().Be(session.Games[0].LivingCells);
        }

        [Fact]
        public void ToState_ShouldReturnGameSessionState()
        {
            // Arrange
            GameSession session = CreateSession();

            // Act
            GameSessionState state = session.ToState();

            // Assert
            state.Should().NotBeNull();
            state.Version.Should().Be(1);
            state.Games.Should().HaveCount(1);
        }

        [Fact]
        public void FromState_ShouldPreserveCompleteSessionState()
        {
            // Arrange
            GameSession originalSession = CreateSession();
            GameSessionState originalState = originalSession.ToState();

            // Act
            GameSession restoredSession = GameSession.FromState(originalState);
            GameSessionState restoredState = restoredSession.ToState();

            // Assert
            restoredState.Should().BeEquivalentTo(originalState);
        }

        // Create a simple GameSession for testing purposes
        private static GameSession CreateSession()
        {
            var gameOfLife = new GameOfLife.Core.GameOfLife(5, 5);
            var games = new[]
            {
                gameOfLife
            };

            return new GameSession(games);
        }
    }
}
