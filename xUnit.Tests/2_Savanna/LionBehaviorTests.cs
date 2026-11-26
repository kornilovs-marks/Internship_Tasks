using FluentAssertions;
using Savanna.Core.Behaviors;
using Savanna.Core.Models;

namespace xUnit.Tests.Savanna;

[Trait("Category", "Behavior")]
public class LionBehaviorTests
{
    private const int VisionRange = 9;
    private const double Health = 10;

    [Fact]
    public void ChooseAction_WhenNoAntelopeInVision_ShouldWanderWithoutTarget()
    {
        // Arrange
        int expectedStepLength = 1;

        AnimalView lion = CreateView(AnimalKind.Lion, new Position(2, 2));
        WorldSnapshot world = CreateWorld(5, 5, lion);

        var behavior = new LionBehavior(new Random());

        // Act
        AnimalActions action = behavior.ChooseAction(lion, world);

        // Assert
        int stepLength = Math.Abs(action.direction.x) + Math.Abs(action.direction.y);

        stepLength.Should().Be(expectedStepLength);
        action.targetId.Should().BeNull();
    }

    [Fact]
    public void ChooseAction_WhenAntelopeInVision_ShouldMoveTowardAndTargetIt()
    {
        // Arrange
        var expectedDirection = new Position(1, 0);    // antelope is to the right

        AnimalView lion = CreateView(AnimalKind.Lion, new Position(2, 0));
        AnimalView antelope = CreateView(AnimalKind.Antelope, new Position(6, 0));
        WorldSnapshot world = CreateWorld(10, 1, lion, antelope);

        var behavior = new LionBehavior(new Random());

        // Act
        AnimalActions action = behavior.ChooseAction(lion, world);

        // Assert
        action.direction.Should().Be(expectedDirection);
        action.targetId.Should().Be(antelope.Id);
    }

    [Fact]
    public void ChooseAction_WhenSeveralAntelopes_ShouldTargetNearest()
    {
        // Arrange
        var expectedDirection = new Position(-1, 0);    // nearest antelope is to the left

        AnimalView lion = CreateView(AnimalKind.Lion, new Position(5, 0));
        AnimalView nearAntelope = CreateView(AnimalKind.Antelope, new Position(3, 0));   // distance 2
        AnimalView farAntelope = CreateView(AnimalKind.Antelope, new Position(9, 0));    // distance 4
        WorldSnapshot world = CreateWorld(10, 1, lion, farAntelope, nearAntelope);

        var behavior = new LionBehavior(new Random());

        // Act
        AnimalActions action = behavior.ChooseAction(lion, world);

        // Assert
        action.targetId.Should().Be(nearAntelope.Id);
        action.direction.Should().Be(expectedDirection);
    }

    [Fact]
    public void ChooseAction_WhenAntelopeOnSameCell_ShouldStayAndTargetIt()
    {
        // Arrange
        var sharedCell = new Position(3, 3);
        var expectedDirection = new Position(0, 0);

        AnimalView lion = CreateView(AnimalKind.Lion, sharedCell);
        AnimalView antelope = CreateView(AnimalKind.Antelope, sharedCell);
        WorldSnapshot world = CreateWorld(5, 5, lion, antelope);

        var behavior = new LionBehavior(new Random());

        // Act
        AnimalActions action = behavior.ChooseAction(lion, world);

        // Assert
        action.direction.Should().Be(expectedDirection);
        action.targetId.Should().Be(antelope.Id);
    }

    // Initialize a world of the given size containing the given animals
    private static WorldSnapshot CreateWorld(int width, int height, params AnimalView[] animals)
    {
        return new WorldSnapshot(width, height, animals);
    }

    // Initialize an animal view of the given kind at the given position
    private static AnimalView CreateView(AnimalKind kind, Position position)
    {
        return new AnimalView(Guid.NewGuid(), kind, position, 5, VisionRange, Health);
    }
}