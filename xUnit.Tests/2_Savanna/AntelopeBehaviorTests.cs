using FluentAssertions;
using Savanna.Core.Behaviors;
using Savanna.Core.Models;

namespace xUnit.Tests.Savanna;

[Trait("Category", "Behavior")]
public class AntelopeBehaviorTests
{
    private const int Repetitions = 50;

    [Fact]
    public void ChooseAction_WhenNoLionInVision_ShouldReturnSingleStep()
    {
        // Arrange
        int visionRange = 7;
        int expectedStepLength = 1;

        AnimalView antelope = CreateView(AnimalKind.Antelope, new Position(2, 2), visionRange);
        WorldSnapshot world = CreateWorld(5, 5, antelope);

        var behavior = new AntelopeBehavior(new Random());

        // Act
        AnimalActions action = behavior.ChooseAction(antelope, world);

        // Assert
        int stepLength = Math.Abs(action.direction.x) + Math.Abs(action.direction.y);

        stepLength.Should().Be(expectedStepLength);
        action.targetId.Should().BeNull();
    }

    [Fact]
    public void ChooseAction_WhenLionInVision_ShouldMoveAway()
    {
        // Arrange
        int visionRange = 7;
        var expectedDirection = new Position(-1, 0);    // lion is to the right, so flee left

        AnimalView antelope = CreateView(AnimalKind.Antelope, new Position(5, 0), visionRange);
        AnimalView lion = CreateView(AnimalKind.Lion, new Position(7, 0), visionRange);
        WorldSnapshot world = CreateWorld(10, 1, antelope, lion);

        var behavior = new AntelopeBehavior(new Random());

        // Act
        AnimalActions action = behavior.ChooseAction(antelope, world);

        // Assert
        action.direction.Should().Be(expectedDirection);
        action.targetId.Should().BeNull();
    }

    [Fact]
    public void ChooseAction_WhenLionIsOutsideVision_ShouldIgnoreIt()
    {
        // Arrange
        int visionRange = 3;
        var towardLion = new Position(1, 0);

        AnimalView antelope = CreateView(AnimalKind.Antelope, new Position(2, 0), visionRange);
        AnimalView lion = CreateView(AnimalKind.Lion, new Position(9, 0), visionRange);   // distance 7, outside vision
        WorldSnapshot world = CreateWorld(10, 1, antelope, lion);

        var behavior = new AntelopeBehavior(new Random());
        var directions = new List<Position>();

        // Act
        for (int i = 0; i < Repetitions; i++)
            directions.Add(behavior.ChooseAction(antelope, world).direction);

        // Assert
        directions.Should().Contain(towardLion);
    }

    // Initialize a world of the given size containing the given animals
    private static WorldSnapshot CreateWorld(int width, int height, params AnimalView[] animals)
    {
        return new WorldSnapshot(width, height, animals);
    }

    // Initialize an animal view of the given kind at the given position
    private static AnimalView CreateView(AnimalKind kind, Position position, int visionRange)
    {
        return new AnimalView(Guid.NewGuid(), kind, position, 4, visionRange);
    }
}