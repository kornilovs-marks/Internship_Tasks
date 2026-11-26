using FluentAssertions;
using Savanna.AnimalsBase.Factories;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Configuration;
using Savanna.Core.Models;
using Savanna.Core.Services;
using xUnit.Tests.Savanna.TestDoubles;

namespace xUnit.Tests.Savanna;

// Tests the GameWorld container. Species and behaviors are test doubles, so no real animal is involved.
[Trait("Category", "World")]
public class GameWorldTests
{
    private const int Width = 5;
    private const int Height = 4;

    [Fact]
    public void Constructor_ShouldTakeSizeFromOptions()
    {
        // Arrange
        var options = new GameOptions { FieldWidth = Width, FieldHeight = Height };

        // Act
        var world = new GameWorld(options, CreateRegistry());

        // Assert
        world.Width.Should().Be(Width);
        world.Height.Should().Be(Height);
    }

    [Fact]
    public void Constructor_WhenOptionsAreInvalid_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidOptions = new GameOptions { FieldWidth = 0 };

        // Act
        Action act = () => new GameWorld(invalidOptions, CreateRegistry());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0, 0)]                     // top-left corner
    [InlineData(Width - 1, 0)]             // top-right corner
    [InlineData(0, Height - 1)]            // bottom-left corner
    [InlineData(Width - 1, Height - 1)]    // bottom-right corner
    [InlineData(2, 2)]                     // middle
    public void Contains_WhenPositionIsInsideField_ShouldReturnTrue(int column, int row)
    {
        // Arrange
        GameWorld world = CreateWorld();
        var position = new Position(column, row);

        // Act
        bool result = world.Contains(position);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1, 0)]        // left of field
    [InlineData(Width, 0)]     // right of field
    [InlineData(0, -1)]        // above field
    [InlineData(0, Height)]    // below field
    public void Contains_WhenPositionIsOutsideField_ShouldReturnFalse(int column, int row)
    {
        // Arrange
        GameWorld world = CreateWorld();
        var position = new Position(column, row);

        // Act
        bool result = world.Contains(position);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Add_WhenPositionIsValid_ShouldAppearInSnapshot()
    {
        // Arrange
        int expectedCount = 1;

        GameWorld world = CreateWorld();
        Animal predator = CreateAnimal(Role.Predator, new Position(1, 1));

        // Act
        world.Add(predator);

        // Assert
        var animals = world.Snapshot().Animals;

        animals.Should().HaveCount(expectedCount);
        animals[0].Id.Should().Be(predator.Id);
    }

    [Fact]
    public void Add_WhenPositionIsOutsideField_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        GameWorld world = CreateWorld();
        Animal predator = CreateAnimal(Role.Predator, new Position(Width, 0));

        // Act
        Action act = () => world.Add(predator);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
        world.Snapshot().Animals.Should().BeEmpty();
    }

    [Fact]
    public void Move_WhenDestinationIsValid_ShouldUpdatePosition()
    {
        // Arrange
        var start = new Position(1, 1);
        var destination = new Position(2, 1);

        GameWorld world = CreateWorld();
        Animal prey = CreateAnimal(Role.Prey, start);
        world.Add(prey);

        // Act
        world.Move(prey, destination);

        // Assert
        prey.Position.Should().Be(destination);
        world.Snapshot().Animals[0].Position.Should().Be(destination);
    }

    [Fact]
    public void Move_WhenDestinationIsOutsideField_ShouldThrowAndKeepOldPosition()
    {
        // Arrange
        var start = new Position(0, 0);
        var outside = new Position(-1, 0);

        GameWorld world = CreateWorld();
        Animal prey = CreateAnimal(Role.Prey, start);
        world.Add(prey);

        // Act
        Action act = () => world.Move(prey, outside);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
        prey.Position.Should().Be(start);
    }

    [Fact]
    public void Remove_WhenAnimalExists_ShouldRemoveItFromSnapshot()
    {
        // Arrange
        GameWorld world = CreateWorld();
        Animal prey = CreateAnimal(Role.Prey, new Position(1, 1));
        world.Add(prey);

        // Act
        world.Remove(prey);

        // Assert
        world.Snapshot().Animals.Should().BeEmpty();
    }

    [Fact]
    public void Remove_WhenSeveralAnimals_ShouldRemoveOnlyThatAnimal()
    {
        // Arrange
        int expectedCount = 1;

        GameWorld world = CreateWorld();
        Animal predator = CreateAnimal(Role.Predator, new Position(0, 0));
        Animal prey = CreateAnimal(Role.Prey, new Position(1, 1));
        world.Add(predator);
        world.Add(prey);

        // Act
        world.Remove(prey);

        // Assert
        var animals = world.Snapshot().Animals;

        animals.Should().HaveCount(expectedCount);
        animals[0].Id.Should().Be(predator.Id);
    }

    [Fact]
    public void Remove_WhenAnimalWasNeverAdded_ShouldNotThrow()
    {
        // Arrange
        GameWorld world = CreateWorld();
        Animal prey = CreateAnimal(Role.Prey, new Position(1, 1));

        // Act
        Action act = () => world.Remove(prey);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Snapshot_ShouldHaveSameSizeAsWorld()
    {
        // Arrange
        GameWorld world = CreateWorld();

        // Act
        WorldSnapshot snapshot = world.Snapshot();

        // Assert
        snapshot.Width.Should().Be(world.Width);
        snapshot.Height.Should().Be(world.Height);
    }

    [Fact]
    public void Snapshot_ShouldKnowWhoEatsWhomFromRegistry()
    {
        // Arrange
        GameWorld world = CreateWorld();

        // Act
        WorldSnapshot snapshot = world.Snapshot();

        // Assert
        snapshot.CanEat(TestSpecies.PredatorId, TestSpecies.PreyId).Should().BeTrue();
        snapshot.CanEat(TestSpecies.PreyId, TestSpecies.PredatorId).Should().BeFalse();
    }

    [Fact]
    public void Snapshot_WhenAnimalMovesAfterwards_ShouldKeepOldPosition()
    {
        // Arrange
        var start = new Position(1, 1);
        var destination = new Position(2, 1);

        GameWorld world = CreateWorld();
        Animal prey = CreateAnimal(Role.Prey, start);
        world.Add(prey);

        WorldSnapshot snapshot = world.Snapshot();

        // Act
        world.Move(prey, destination);

        // Assert
        snapshot.Animals[0].Position.Should().Be(start);
    }

    [Fact]
    public void Snapshot_WhenAnimalAddedAfterwards_ShouldNotContainIt()
    {
        // Arrange
        GameWorld world = CreateWorld();
        WorldSnapshot snapshot = world.Snapshot();

        // Act
        world.Add(CreateAnimal(Role.Predator, new Position(0, 0)));

        // Assert
        snapshot.Animals.Should().BeEmpty();
    }

    // Initialize a registry that knows the test species
    private static SpeciesRegistry CreateRegistry()
    {
        AnimalFactoryBase[] factories =
        [
            new StubFactory(TestSpecies.Prey()),
            new StubFactory(TestSpecies.Predator())
        ];

        return new SpeciesRegistry(factories);
    }

    // Initialize a small world used by every test
    private static GameWorld CreateWorld()
    {
        var options = new GameOptions { FieldWidth = Width, FieldHeight = Height };

        return new GameWorld(options, CreateRegistry());
    }

    // Initialize an animal that plays the given role at the given position
    private static Animal CreateAnimal(Role role, Position position)
    {
        return new Animal(TestSpecies.For(role), position, StubBehavior.StandStill());
    }
}
