using FluentAssertions;
using Moq;
using Savanna.Core.Behaviors;
using Savanna.Core.Configuration;
using Savanna.Core.Factories;
using Savanna.Core.Interfaces;
using Savanna.Core.Models;
using Savanna.Core.Services;

namespace xUnit.Tests.Savanna;

[Trait("Category", "GameLogic")]
public class GameEngineTests
{
    private const int Vision = 9;
    private const double StandStillSpeed = 1;    // 0.1 credit per tick, never moves in one tick
    private const double OneStepSpeed = 10;      // 1.0 credit per tick, exactly one step per tick

    private static readonly TimeSpan Tick = TimeSpan.FromMilliseconds(100);

    [Fact]
    public void TryAddAnimal_WhenFieldHasSpace_ShouldReturnTrueAndAddAnimal()
    {
        // Arrange
        int expectedCount = 1;

        var random = new Random();
        GameWorld world = CreateWorld(5, 5);

        var factoryMock = new Mock<IAnimalFactory>();
        factoryMock
            .Setup(factory => factory.Create(AnimalKind.Antelope, It.IsAny<Position>()))
            .Returns((AnimalKind _, Position position) => CreateAntelope(position, StandStillSpeed));

        var engine = new GameEngine(world, factoryMock.Object, random);

        // Act
        bool added = engine.TryAddAnimal(AnimalKind.Antelope);

        // Assert
        added.Should().BeTrue();
        world.Snapshot().Animals.Should().HaveCount(expectedCount);
        factoryMock.Verify(
            factory => factory.Create(AnimalKind.Antelope, It.IsAny<Position>()), Times.Once);
    }

    [Fact]
    public void TryAddAnimal_WhenFieldIsFull_ShouldReturnFalse()
    {
        // Arrange
        int expectedCount = 1;

        GameWorld world = CreateWorld(1, 1);
        GameEngine engine = CreateEngine(world);

        engine.TryAddAnimal(AnimalKind.Antelope);

        // Act
        bool added = engine.TryAddAnimal(AnimalKind.Lion);

        // Assert
        added.Should().BeFalse();
        world.Snapshot().Animals.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void TryAddAnimal_WhenCalledManyTimes_ShouldNeverShareCells()
    {
        // Arrange
        int width = 3;
        int height = 3;
        int cellCount = width * height;

        GameWorld world = CreateWorld(width, height);
        GameEngine engine = CreateEngine(world);

        // Act
        for (int i = 0; i < cellCount; i++)
            engine.TryAddAnimal(AnimalKind.Antelope);

        // Assert
        var positions = world.Snapshot().Animals.Select(animal => animal.Position).ToList();

        positions.Should().HaveCount(cellCount);
        positions.Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Tick_WhenElapsedIsNotPositive_ShouldThrowArgumentOutOfRangeException(int milliseconds)
    {
        // Arrange
        GameEngine engine = CreateEngine(CreateWorld(5, 5));
        var elapsed = TimeSpan.FromMilliseconds(milliseconds);

        // Act
        Action act = () => engine.Tick(elapsed);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Tick_WhenCreditIsBelowOne_ShouldNotMoveAnimal()
    {
        // Arrange
        double speed = 4;    // 0.4 credit per tick, 0.8 after two ticks
        int ticks = 2;

        var start = new Position(2, 2);
        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAntelope(start, speed));

        GameEngine engine = CreateEngine(world);

        // Act
        for (int i = 0; i < ticks; i++)
            engine.Tick(Tick);

        // Assert
        world.Snapshot().Animals[0].Position.Should().Be(start);
    }

    [Fact]
    public void Tick_WhenCreditReachesOne_ShouldMoveExactlyOneCell()
    {
        // Arrange
        double speed = 4;    // 1.2 credit on the third tick
        int ticks = 3;
        int expectedDistance = 1;

        var start = new Position(2, 2);
        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAntelope(start, speed));

        GameEngine engine = CreateEngine(world);

        // Act
        for (int i = 0; i < ticks; i++)
            engine.Tick(Tick);

        // Assert
        world.Snapshot().Animals[0].Position.DistanceTo(start).Should().Be(expectedDistance);
    }

    [Fact]
    public void Tick_WhenLionSeesAntelope_ShouldMoveCloser()
    {
        // Arrange
        GameWorld world = CreateWorld(10, 1);
        Lion lion = CreateLion(new Position(0, 0), OneStepSpeed);
        Antelope antelope = CreateAntelope(new Position(5, 0), StandStillSpeed);
        world.Add(lion);
        world.Add(antelope);

        GameEngine engine = CreateEngine(world);
        int distanceBefore = lion.Position.DistanceTo(antelope.Position);

        // Act
        engine.Tick(Tick);

        // Assert
        lion.Position.DistanceTo(antelope.Position).Should().BeLessThan(distanceBefore);
    }

    [Fact]
    public void Tick_WhenAntelopeSeesLion_ShouldMoveAway()
    {
        // Arrange
        GameWorld world = CreateWorld(10, 1);
        Lion lion = CreateLion(new Position(3, 0), StandStillSpeed);
        Antelope antelope = CreateAntelope(new Position(5, 0), OneStepSpeed);
        world.Add(lion);
        world.Add(antelope);

        GameEngine engine = CreateEngine(world);
        int distanceBefore = antelope.Position.DistanceTo(lion.Position);

        // Act
        engine.Tick(Tick);

        // Assert
        antelope.Position.DistanceTo(lion.Position).Should().BeGreaterThan(distanceBefore);
    }

    [Fact]
    public void Tick_WhenLionSharesCellWithAntelope_ShouldRemoveAntelope()
    {
        // Arrange
        int expectedCount = 1;

        var sharedCell = new Position(2, 0);
        GameWorld world = CreateWorld(5, 1);
        world.Add(CreateLion(sharedCell, StandStillSpeed));
        world.Add(CreateAntelope(sharedCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        var animals = world.Snapshot().Animals;

        animals.Should().HaveCount(expectedCount);
        animals[0].Kind.Should().Be(AnimalKind.Lion);
    }

    [Fact]
    public void Tick_WhenTwoLionsShareCellWithOneAntelope_ShouldRemoveOnlyThatAntelope()
    {
        // Arrange
        int expectedCount = 2;

        var sharedCell = new Position(2, 0);
        GameWorld world = CreateWorld(5, 1);
        world.Add(CreateLion(sharedCell, StandStillSpeed));
        world.Add(CreateLion(sharedCell, StandStillSpeed));
        world.Add(CreateAntelope(sharedCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world);

        // Act
        Action act = () => engine.Tick(Tick);

        // Assert
        act.Should().NotThrow();

        var animals = world.Snapshot().Animals;

        animals.Should().HaveCount(expectedCount);
        animals.Should().OnlyContain(animal => animal.Kind == AnimalKind.Lion);
    }

    [Fact]
    public void Tick_WhenAnimalIsAtEdge_ShouldStayInsideField()
    {
        // Arrange
        int ticks = 100;

        GameWorld world = CreateWorld(3, 3);
        world.Add(CreateAntelope(new Position(0, 0), OneStepSpeed));

        GameEngine engine = CreateEngine(world);

        for (int i = 0; i < ticks; i++)
        {
            // Act
            engine.Tick(Tick);

            // Assert
            world.Contains(world.Snapshot().Animals[0].Position).Should().BeTrue();
        }
    }

    // Initialize a world of the given size
    private static GameWorld CreateWorld(int width, int height)
    {
        var options = new GameOptions { FieldWidth = width, FieldHeight = height };

        return new GameWorld(options);
    }

    // Initialize an engine with the real factories
    private static GameEngine CreateEngine(GameWorld world)
    {
        var options = new GameOptions();
        var random = new Random();
        var factories = new AnimalFactoryBase[]
        {
            new AntelopeFactory(options, random),
            new LionFactory(options, random)
        };

        return new GameEngine(world, new AnimalFactory(factories), random);
    }

    // Initialize a lion with the given position and speed
    private static Lion CreateLion(Position position, double speed)
    {
        return new Lion(position, speed, Vision, new LionBehavior(new Random()));
    }

    // Initialize an antelope with the given position and speed
    private static Antelope CreateAntelope(Position position, double speed)
    {
        return new Antelope(position, speed, Vision, new AntelopeBehavior(new Random()));
    }
}