using FluentAssertions;
using Savanna.Core.Models;

namespace xUnit.Tests.Savanna;

[Trait("Category", "Models")]
public class PositionTests
{
    [Fact]
    public void DistanceTo_WhenPositionsDiffer_ShouldReturnManhattanDistance()
    {
        // Arrange
        int startColumn = 1;
        int startRow = 1;
        int targetColumn = 4;
        int targetRow = 5;

        int expectedDistance = 7;

        var start = new Position(startColumn, startRow);
        var target = new Position(targetColumn, targetRow);

        // Act
        int distance = start.DistanceTo(target);

        // Assert
        distance.Should().Be(expectedDistance);
    }

    [Fact]
    public void DistanceTo_WhenSamePosition_ShouldReturnZero()
    {
        // Arrange
        int column = 3;
        int row = 2;

        int expectedDistance = 0;

        var position = new Position(column, row);
        var samePosition = new Position(column, row);

        // Act
        int distance = position.DistanceTo(samePosition);

        // Assert
        distance.Should().Be(expectedDistance);
    }

    [Fact]
    public void DistanceTo_WhenArgumentsSwapped_ShouldReturnSameDistance()
    {
        // Arrange
        var first = new Position(0, 6);
        var second = new Position(5, 2);

        // Act
        int forwardDistance = first.DistanceTo(second);
        int backwardDistance = second.DistanceTo(first);

        // Assert
        forwardDistance.Should().Be(backwardDistance);
    }

    [Theory]
    [InlineData(2, 3, 1, 0, 3, 3)]    // step right
    [InlineData(2, 3, -1, 0, 1, 3)]   // step left
    [InlineData(2, 3, 0, 1, 2, 4)]    // step down
    [InlineData(2, 3, 0, -1, 2, 2)]   // step up
    [InlineData(2, 3, 0, 0, 2, 3)]    // stay
    public void PlusOperator_ShouldAddCoordinates(
        int originColumn, int originRow,
        int directionColumn, int directionRow,
        int expectedColumn, int expectedRow)
    {
        // Arrange
        var origin = new Position(originColumn, originRow);
        var direction = new Position(directionColumn, directionRow);
        var expected = new Position(expectedColumn, expectedRow);

        // Act
        Position result = origin + direction;

        // Assert
        result.Should().Be(expected);
    }
}