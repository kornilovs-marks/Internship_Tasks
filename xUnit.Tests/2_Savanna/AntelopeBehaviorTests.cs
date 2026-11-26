using FluentAssertions;
using Savanna.AnimalsBase.Factories;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Behaviors;
using Savanna.Core.Models;
using Savanna.Core.Services;
using xUnit.Tests.Savanna.TestDoubles;

namespace xUnit.Tests.Savanna;

[Trait("Category", "Behavior")]
public class AntelopeBehaviorTests
{
    private const int Repetitions = 50;
    private const double Health = TestSpecies.DefaultHealth;

    [Fact]
    public void ChooseAction_WhenNoThreatInVision_ShouldReturnSingleStep()
    {
        // Arrange
        int visionRange = 7;
        int expectedStepLength = 1;

        AnimalView antelope = CreateView(TestSpecies.PreyId, new Position(2, 2), visionRange);
        WorldSnapshot world = CreateWorld(5, 5, [antelope]);

        var behavior = new AntelopeBehavior(new Random());

        // Act
        AnimalActions action = behavior.ChooseAction(antelope, world);

        // Assert
        action.direction.Length.Should().Be(expectedStepLength);
        action.targetId.Should().BeNull();
    }

    [Fact]
    public void ChooseAction_WhenPredatorInVision_ShouldMoveAway()
    {
        // Arrange
        int visionRange = 7;
        var expectedDirection = StubBehavior.Left;    // predator is to the right, so flee left

        AnimalView antelope = CreateView(TestSpecies.PreyId, new Position(5, 0), visionRange);
        AnimalView predator = CreateView(TestSpecies.PredatorId, new Position(7, 0), visionRange);
        WorldSnapshot world = CreateWorld(10, 1, [antelope, predator]);

        var behavior = new AntelopeBehavior(new Random());

        // Act
        AnimalActions action = behavior.ChooseAction(antelope, world);

        // Assert
        action.direction.Should().Be(expectedDirection);
        action.targetId.Should().BeNull();
    }

    [Fact]
    public void ChooseAction_WhenPredatorIsOutsideVision_ShouldIgnoreIt()
    {
        // Arrange
        int visionRange = 3;
        var towardPredator = StubBehavior.Right;

        AnimalView antelope = CreateView(TestSpecies.PreyId, new Position(2, 0), visionRange);
        AnimalView predator = CreateView(TestSpecies.PredatorId, new Position(9, 0), visionRange);   // distance 7, outside vision
        WorldSnapshot world = CreateWorld(10, 1, [antelope, predator]);

        var behavior = new AntelopeBehavior(new Random());

        // Act
        List<Position> directions = ChooseManyTimes(behavior, antelope, world);

        // Assert
        directions.Should().Contain(towardPredator);
    }

    [Fact]
    public void ChooseAction_WhenNearbyAnimalCannotEatIt_ShouldIgnoreIt()
    {
        // Arrange
        int visionRange = 7;
        var towardNeighbour = StubBehavior.Right;

        AnimalView antelope = CreateView(TestSpecies.PreyId, new Position(2, 0), visionRange);
        AnimalView neighbour = CreateView(TestSpecies.PreyId, new Position(4, 0), visionRange);   // same species, not a threat
        WorldSnapshot world = CreateWorld(10, 1, [antelope, neighbour]);

        var behavior = new AntelopeBehavior(new Random());

        // Act
        List<Position> directions = ChooseManyTimes(behavior, antelope, world);

        // Assert
        directions.Should().Contain(towardNeighbour);
    }

    [Fact]
    public void ChooseAction_WhenNewPredatorSpeciesInVision_ShouldMoveAway()
    {
        // Arrange: a species the behavior was never written for, like an imported cheetah
        int visionRange = 7;
        var expectedDirection = StubBehavior.Left;    // new predator is to the right, so flee left

        var importedId = new SpeciesId("TestImportedPredator");
        var imported = new SpeciesDescriptor(
            importedId, "Imported", "II", 'I', TestSpecies.DefaultSpeed, visionRange, Health,
            new HashSet<SpeciesId> { TestSpecies.PreyId });

        AnimalView antelope = CreateView(TestSpecies.PreyId, new Position(5, 0), visionRange);
        AnimalView newPredator = CreateView(importedId, new Position(7, 0), visionRange);
        WorldSnapshot world = CreateWorld(10, 1, [antelope, newPredator], new StubFactory(imported));

        var behavior = new AntelopeBehavior(new Random());

        // Act
        AnimalActions action = behavior.ChooseAction(antelope, world);

        // Assert
        action.direction.Should().Be(expectedDirection);
    }

    // Runs the random behavior many times so a test can check which directions it ever chooses
    private static List<Position> ChooseManyTimes(AntelopeBehavior behavior, AnimalView antelope, WorldSnapshot world)
    {
        var directions = new List<Position>();

        for (int i = 0; i < Repetitions; i++)
            directions.Add(behavior.ChooseAction(antelope, world).direction);

        return directions;
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
    private static AnimalView CreateView(SpeciesId species, Position position, int visionRange)
    {
        return new AnimalView(Guid.NewGuid(), species, position, TestSpecies.DefaultSpeed, visionRange, Health);
    }
}
