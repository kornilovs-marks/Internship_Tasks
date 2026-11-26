using FluentAssertions;
using Savanna.AnimalsBase.Factories;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Behaviors;
using Savanna.Core.Models;
using Savanna.Core.Services;
using xUnit.Tests.Savanna.TestDoubles;

namespace xUnit.Tests.Savanna;

[Trait("Category", "Behavior")]
public class LionBehaviorTests
{
    private const int VisionRange = 9;
    private const double Health = TestSpecies.DefaultHealth;

    [Fact]
    public void ChooseAction_WhenNoPreyInVision_ShouldWanderWithoutTarget()
    {
        // Arrange
        int expectedStepLength = 1;

        AnimalView lion = CreateView(TestSpecies.PredatorId, new Position(2, 2));
        WorldSnapshot world = CreateWorld(5, 5, [lion]);

        var behavior = new LionBehavior(new Random());

        // Act
        AnimalActions action = behavior.ChooseAction(lion, world);

        // Assert
        action.direction.Length.Should().Be(expectedStepLength);
        action.targetId.Should().BeNull();
    }

    [Fact]
    public void ChooseAction_WhenPreyInVision_ShouldMoveTowardAndTargetIt()
    {
        // Arrange
        var expectedDirection = StubBehavior.Right;    // prey is to the right

        AnimalView lion = CreateView(TestSpecies.PredatorId, new Position(2, 0));
        AnimalView prey = CreateView(TestSpecies.PreyId, new Position(6, 0));
        WorldSnapshot world = CreateWorld(10, 1, [lion, prey]);

        var behavior = new LionBehavior(new Random());

        // Act
        AnimalActions action = behavior.ChooseAction(lion, world);

        // Assert
        action.direction.Should().Be(expectedDirection);
        action.targetId.Should().Be(prey.Id);
    }

    [Fact]
    public void ChooseAction_WhenSeveralPrey_ShouldTargetNearest()
    {
        // Arrange
        var expectedDirection = StubBehavior.Left;    // nearest prey is to the left

        AnimalView lion = CreateView(TestSpecies.PredatorId, new Position(5, 0));
        AnimalView nearPrey = CreateView(TestSpecies.PreyId, new Position(3, 0));   // distance 2
        AnimalView farPrey = CreateView(TestSpecies.PreyId, new Position(9, 0));    // distance 4
        WorldSnapshot world = CreateWorld(10, 1, [lion, farPrey, nearPrey]);

        var behavior = new LionBehavior(new Random());

        // Act
        AnimalActions action = behavior.ChooseAction(lion, world);

        // Assert
        action.targetId.Should().Be(nearPrey.Id);
        action.direction.Should().Be(expectedDirection);
    }

    [Fact]
    public void ChooseAction_WhenPreyOnSameCell_ShouldStayAndTargetIt()
    {
        // Arrange
        var sharedCell = new Position(3, 3);
        var expectedDirection = new Position(0, 0);

        AnimalView lion = CreateView(TestSpecies.PredatorId, sharedCell);
        AnimalView prey = CreateView(TestSpecies.PreyId, sharedCell);
        WorldSnapshot world = CreateWorld(5, 5, [lion, prey]);

        var behavior = new LionBehavior(new Random());

        // Act
        AnimalActions action = behavior.ChooseAction(lion, world);

        // Assert
        action.direction.Should().Be(expectedDirection);
        action.targetId.Should().Be(prey.Id);
    }

    // Initialize a world of the given size containing the given animals; it knows the test species plus any extra ones
    private static WorldSnapshot CreateWorld(int width, int height, AnimalView[] animals, params AnimalFactoryBase[] extraSpecies)
    {
        AnimalFactoryBase[] factories =
        [
            new StubFactory(TestSpecies.Prey()),
            new StubFactory(TestSpecies.Predator()),
            .. extraSpecies
        ];

        return new WorldSnapshot(width, height, animals, new SpeciesRegistry(factories));
    }

    // Initialize an animal view of the given species at the given position
    private static AnimalView CreateView(SpeciesId species, Position position)
    {
        return new AnimalView(Guid.NewGuid(), species, position, TestSpecies.DefaultSpeed, VisionRange, Health);
    }
}
