using FluentAssertions;
using GameOfLife.Core;

namespace xUnit.Tests._1_GameOfLife
{
    [Trait("Category", "GameLogic")]
    public class GameOfLifeTests
    {
        [Fact]
        public void NextGeneration_WhenPatternIsStableBlock_ShouldRemainUnchanged()
        {
            // Arrange
            int firstRow = 1;
            int secondRow = 2;
            int firstColumn = 1;
            int secondColumn = 2;

            int expectedLivingCells = 4;
            long expectedIteration = 1;

            GameOfLife.Core.GameOfLife game = CreateBlockGame();

            // Act
            game.NextGeneration();

            // Assert
            game.IsAlive(firstRow, firstColumn).Should().BeTrue();
            game.IsAlive(firstRow, secondColumn).Should().BeTrue();
            game.IsAlive(secondRow, firstColumn).Should().BeTrue();
            game.IsAlive(secondRow, secondColumn).Should().BeTrue();

            game.LivingCells.Should().Be(expectedLivingCells);
            game.Iteration.Should().Be(expectedIteration);
        }

        [Fact]
        public void NextGeneration_WhenPatternIsHorizontalBlinker_ShouldBecomeVerticalBlinker()
        {
            // Arrange
            int firstRow = 1;
            int middleRow = 2;
            int lastRow = 3;

            int firstColumn = 1;
            int middleColumn = 2;
            int lastColumn = 3;

            int expectedLivingCells = 3;
            long expectedIteration = 1;

            GameOfLife.Core.GameOfLife game = CreateHorizontalBlinker();

            // Act
            game.NextGeneration();

            // Assert
            game.IsAlive(firstRow, middleColumn).Should().BeTrue();
            game.IsAlive(middleRow, middleColumn).Should().BeTrue();
            game.IsAlive(lastRow, middleColumn).Should().BeTrue();

            game.IsAlive(middleRow, firstColumn).Should().BeFalse();
            game.IsAlive(middleRow, lastColumn).Should().BeFalse();
            
            game.LivingCells.Should().Be(expectedLivingCells);
            game.Iteration.Should().Be(expectedIteration);
        }

        [Fact]
        public void ToStateAndFromState_ShouldPreserveGameState()
        {
            // Arrange
            GameOfLife.Core.GameOfLife originalGame = CreateHorizontalBlinker();
            GameState originalState = originalGame.ToState();

            // Act
            GameOfLife.Core.GameOfLife restoredGame = GameOfLife.Core.GameOfLife.FromState(originalState);

            GameState restoredState = restoredGame.ToState();

            // Assert
            restoredState.Should().BeEquivalentTo(originalState);
            restoredGame.LivingCells.Should().Be(3);
        }

        [Fact]
        public void FromState_WhenCellCountDoesNotMatchDimensions_ShouldThrowArgumentException()
        {
            int width = 4;
            int height = 4;

            long iteration = 0;
            int invalidCellCount = width * height - 1;

            var cells = new bool[invalidCellCount];
            // Arrange
            var invalidState = new GameState(width, height, iteration, cells);

            // Act
            Action act = () => GameOfLife.Core.GameOfLife.FromState(invalidState);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        // Inicialize a simple GameSession with one game
        private static GameOfLife.Core.GameOfLife CreateBlockGame()
        {
            int width = 4;
            int height = 4;

            int firstRow = 1;
            int secondRow = 2;
            int firstColumn = 1;
            int secondColumn = 2;
            
            long iteration = 0;

            var cells = new bool[width * height];

            cells[firstRow * width + firstColumn] = true;
            cells[firstRow * width + secondColumn] = true;
            cells[secondRow * width + firstColumn] = true;
            cells[secondRow * width + secondColumn] = true;

            var state = new GameState(width, height, iteration, cells);

            return GameOfLife.Core.GameOfLife.FromState(state);
        }

        // Inicialize a simple GameOfLife with a horizontal blinker pattern to prove that the NextGeneration method works correctly
        private static GameOfLife.Core.GameOfLife CreateHorizontalBlinker()
        {
            int width = 5;
            int height = 5;
            int row = 2;

            int leftColumn = 1;
            int middleColumn = 2;
            int rightColumn = 3;

            long iteration = 0;

            var cells = new bool[width * height];

            // columns 1, 2, 3 in the middle row are alive
            cells[row * width + leftColumn] = true;
            cells[row * width + middleColumn] = true;
            cells[row * width + rightColumn] = true;

            var state = new GameState(width, height, iteration, cells);

            return GameOfLife.Core.GameOfLife.FromState(state);
        }
    }
}