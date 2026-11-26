using FluentAssertions;
using Savanna.AnimalsBase.Behaviors;
using Savanna.AnimalsBase.Factories;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Models;
using Savanna.Core.Services;

namespace xUnit.Tests.Savanna;

[Trait("Category", "Behavior")]
public class DirectionChoicesTests
{
    private const int Width = 5;
    private const int Height = 5;
    private const int Repetitions = 50;

    [Fact]
    public void Wander_WhenInMiddleOfField_ShouldReturnSingleStepDirection()
    {
        // Arrange
        int expectedStepLength = 1;

        var origin = new Position(2, 2);
        WorldSnapshot world = CreateWorld(Width, Height);
        var random = new Random();

        for (int i = 0; i < Repetitions; i++)
        {
            // Act
            Position direction = DirectionChoices.Wander(origin, world, random);

            // Assert
            direction.Length.Should().Be(expectedStepLength);
        }
    }

    [Fact]
    public void Wander_WhenInCorner_ShouldNeverLeaveField()
    {
        // Arrange
        var corner = new Position(0, 0);
        WorldSnapshot world = CreateWorld(Width, Height);
        var random = new Random();

        for (int i = 0; i < Repetitions; i++)
        {
            // Act
            Position direction = DirectionChoices.Wander(corner, world, random);

            // Assert
            world.Contains(corner + direction).Should().BeTrue();
        }
    }

    [Fact]
    public void Wander_WhenFieldIsOneCell_ShouldReturnZeroDirection()
    {
        // Arrange
        var origin = new Position(0, 0);
        var expectedDirection = new Position(0, 0);

        WorldSnapshot world = CreateWorld(1, 1);
        var random = new Random();

        // Act
        Position direction = DirectionChoices.Wander(origin, world, random);

        // Assert
        direction.Should().Be(expectedDirection);
    }

    [Fact]
    public void TowardOrAway_WhenMovingToward_ShouldReduceDistance()
    {
        // Arrange
        var origin = new Position(1, 2);
        var target = new Position(4, 2);              // same row, so only "right" gets closer
        var expectedDirection = new Position(1, 0);

        WorldSnapshot world = CreateWorld(Width, Height);
        var random = new Random();

        int distanceBefore = origin.DistanceTo(target);

        // Act
        Position direction = DirectionChoices.TowardOrAway(origin, target, world, random, away: false);

        // Assert
        direction.Should().Be(expectedDirection);
        (origin + direction).DistanceTo(target).Should().BeLessThan(distanceBefore);
    }

    [Fact]
    public void TowardOrAway_WhenMovingAway_ShouldIncreaseDistance()
    {
        // Arrange
        var origin = new Position(2, 2);
        var target = new Position(4, 2);

        WorldSnapshot world = CreateWorld(Width, Height);
        var random = new Random();

        int distanceBefore = origin.DistanceTo(target);

        for (int i = 0; i < Repetitions; i++)
        {
            // Act
            Position direction = DirectionChoices.TowardOrAway(origin, target, world, random, away: true);

            // Assert
            (origin + direction).DistanceTo(target).Should().BeGreaterThan(distanceBefore);
        }
    }

    [Fact]
    public void TowardOrAway_WhenFleeingInCorner_ShouldStayInsideField()
    {
        // Arrange
        var corner = new Position(0, 0);
        var threat = new Position(1, 1);

        WorldSnapshot world = CreateWorld(Width, Height);
        var random = new Random();

        for (int i = 0; i < Repetitions; i++)
        {
            // Act
            Position direction = DirectionChoices.TowardOrAway(corner, threat, world, random, away: true);

            // Assert
            world.Contains(corner + direction).Should().BeTrue();
        }
    }

    // Initialize an empty world of the given size; no species are needed because no animals are in it
    private static WorldSnapshot CreateWorld(int width, int height)
    {
        return new WorldSnapshot(width, height, [], new SpeciesRegistry(Array.Empty<AnimalFactoryBase>()));
    }
}
