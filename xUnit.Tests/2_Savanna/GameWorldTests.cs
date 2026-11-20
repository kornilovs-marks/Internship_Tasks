using FluentAssertions;
using Savanna.Core.Behaviors;
using Savanna.Core.Configuration;
using Savanna.Core.Models;

namespace xUnit.Tests.Savanna;

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
        var world = new GameWorld(options);

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
        Action act = () => new GameWorld(invalidOptions);

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
        Animal lion = CreateLion(new Position(1, 1));

        // Act
        world.Add(lion);

        // Assert
        var animals = world.Snapshot().Animals;

        animals.Should().HaveCount(expectedCount);
        animals[0].Id.Should().Be(lion.Id);
    }

    [Fact]
    public void Add_WhenPositionIsOutsideField_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        GameWorld world = CreateWorld();
        Animal lion = CreateLion(new Position(Width, 0));

        // Act
        Action act = () => world.Add(lion);

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
        Animal antelope = CreateAntelope(start);
        world.Add(antelope);

        // Act
        world.Move(antelope, destination);

        // Assert
        antelope.Position.Should().Be(destination);
        world.Snapshot().Animals[0].Position.Should().Be(destination);
    }

    [Fact]
    public void Move_WhenDestinationIsOutsideField_ShouldThrowAndKeepOldPosition()
    {
        // Arrange
        var start = new Position(0, 0);
        var outside = new Position(-1, 0);

        GameWorld world = CreateWorld();
        Animal antelope = CreateAntelope(start);
        world.Add(antelope);

        // Act
        Action act = () => world.Move(antelope, outside);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
        antelope.Position.Should().Be(start);
    }

    [Fact]
    public void Remove_WhenAnimalExists_ShouldRemoveItFromSnapshot()
    {
        // Arrange
        GameWorld world = CreateWorld();
        Animal antelope = CreateAntelope(new Position(1, 1));
        world.Add(antelope);

        // Act
        world.Remove(antelope);

        // Assert
        world.Snapshot().Animals.Should().BeEmpty();
    }

    [Fact]
    public void Remove_WhenSeveralAnimals_ShouldRemoveOnlyThatAnimal()
    {
        // Arrange
        int expectedCount = 1;

        GameWorld world = CreateWorld();
        Animal lion = CreateLion(new Position(0, 0));
        Animal antelope = CreateAntelope(new Position(1, 1));
        world.Add(lion);
        world.Add(antelope);

        // Act
        world.Remove(antelope);

        // Assert
        var animals = world.Snapshot().Animals;

        animals.Should().HaveCount(expectedCount);
        animals[0].Id.Should().Be(lion.Id);
    }

    [Fact]
    public void Snapshot_WhenAnimalMovesAfterwards_ShouldKeepOldPosition()
    {
        // Arrange
        var start = new Position(1, 1);
        var destination = new Position(2, 1);

        GameWorld world = CreateWorld();
        Animal antelope = CreateAntelope(start);
        world.Add(antelope);

        WorldSnapshot snapshot = world.Snapshot();

        // Act
        world.Move(antelope, destination);

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
        world.Add(CreateLion(new Position(0, 0)));

        // Assert
        snapshot.Animals.Should().BeEmpty();
    }

    // Initialize a small world used by every test
    private static GameWorld CreateWorld()
    {
        var options = new GameOptions { FieldWidth = Width, FieldHeight = Height };

        return new GameWorld(options);
    }

    // Initialize a lion at the given position
    private static Lion CreateLion(Position position)
    {
        return new Lion(position, 5, 9, new LionBehavior(new Random()));
    }

    // Initialize an antelope at the given position
    private static Antelope CreateAntelope(Position position)
    {
        return new Antelope(position, 4, 7, new AntelopeBehavior(new Random()));
    }
}