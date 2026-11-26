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
    private const double Health = 20;
    private const double StandStillSpeed = 0.001;   // never builds a full step during these tests
    private const double OneStepSpeed = 10;         // 1.0 credit per tick, exactly one step per tick

    private static readonly TimeSpan Tick = TimeSpan.FromMilliseconds(100);

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void TryAddAnimal_WhenFieldHasSpace_ShouldReturnTrueAndAddAnimal(AnimalKind kind)
    {
        // Arrange
        int expectedCount = 1;

        var random = new Random();
        var options = new GameOptions();
        GameWorld world = CreateWorld(5, 5);

        var factoryMock = new Mock<IAnimalFactory>();
        factoryMock
            .Setup(factory => factory.Create(kind, It.IsAny<Position>()))
            .Returns((AnimalKind createdKind, Position position) => CreateAnimal(createdKind, position, StandStillSpeed));

        var movement = new AnimalMovement(world, options);
        var birth = new AnimalBirth(world, factoryMock.Object, random, options);
        var engine = new GameEngine(world, factoryMock.Object, random, options, movement, birth);

        // Act
        bool added = engine.TryAddAnimal(kind);

        // Assert
        added.Should().BeTrue();
        world.Snapshot().Animals.Should().HaveCount(expectedCount);
        world.Snapshot().Animals[0].Kind.Should().Be(kind);
        factoryMock.Verify(
            factory => factory.Create(kind, It.IsAny<Position>()), Times.Once);
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

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Tick_WhenCreditIsBelowOne_ShouldNotMoveAnimal(AnimalKind kind)
    {
        // Arrange
        double speed = 4;    // 0.4 credit per tick, 0.8 after two ticks
        int ticks = 2;

        var start = new Position(2, 2);
        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAnimal(kind, start, speed));

        GameEngine engine = CreateEngine(world);

        // Act
        for (int i = 0; i < ticks; i++)
            engine.Tick(Tick);

        // Assert
        world.Snapshot().Animals[0].Position.Should().Be(start);
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Tick_WhenCreditReachesOne_ShouldMoveExactlyOneCell(AnimalKind kind)
    {
        // Arrange
        double speed = 4;    // 1.2 credit on the third tick
        int ticks = 3;
        int expectedDistance = 1;

        var start = new Position(2, 2);
        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAnimal(kind, start, speed));

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
        Animal lion = CreateAnimal(AnimalKind.Lion, new Position(0, 0), OneStepSpeed);
        Animal antelope = CreateAnimal(AnimalKind.Antelope, new Position(5, 0), StandStillSpeed);
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
        Animal lion = CreateAnimal(AnimalKind.Lion, new Position(3, 0), StandStillSpeed);
        Animal antelope = CreateAnimal(AnimalKind.Antelope, new Position(5, 0), OneStepSpeed);
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
        world.Add(CreateAnimal(AnimalKind.Lion, sharedCell, StandStillSpeed));
        world.Add(CreateAnimal(AnimalKind.Antelope, sharedCell, StandStillSpeed));

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
        world.Add(CreateAnimal(AnimalKind.Lion, sharedCell, StandStillSpeed));
        world.Add(CreateAnimal(AnimalKind.Lion, sharedCell, StandStillSpeed));
        world.Add(CreateAnimal(AnimalKind.Antelope, sharedCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world);

        // Act
        Action act = () => engine.Tick(Tick);

        // Assert
        act.Should().NotThrow();

        var animals = world.Snapshot().Animals;

        animals.Should().HaveCount(expectedCount);
        animals.Should().OnlyContain(animal => animal.Kind == AnimalKind.Lion);
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Tick_WhenAnimalIsAtEdge_ShouldStayInsideField(AnimalKind kind)
    {
        // Arrange
        int ticks = 100;
        double enoughHealthForAllSteps = 1000;

        GameWorld world = CreateWorld(3, 3);
        world.Add(CreateAnimal(kind, new Position(0, 0), OneStepSpeed, enoughHealthForAllSteps));

        GameEngine engine = CreateEngine(world);

        for (int i = 0; i < ticks; i++)
        {
            // Act
            engine.Tick(Tick);

            // Assert
            world.Contains(world.Snapshot().Animals[0].Position).Should().BeTrue();
        }
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Tick_WhenAnimalMovesOneCell_ShouldLoseMoveHealthCost(AnimalKind kind)
    {
        // Arrange
        double moveCost = 0.5;
        double expectedHealth = Health - moveCost;

        GameWorld world = CreateWorld(5, 5);
        Animal animal = CreateAnimal(kind, new Position(2, 2), OneStepSpeed);
        world.Add(animal);

        GameEngine engine = CreateEngine(world, new GameOptions { MoveHealthCost = moveCost });

        // Act
        engine.Tick(Tick);

        // Assert
        animal.Health.Should().Be(expectedHealth);
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Tick_WhenAnimalDoesNotMove_ShouldKeepHealth(AnimalKind kind)
    {
        // Arrange
        GameWorld world = CreateWorld(5, 5);
        Animal animal = CreateAnimal(kind, new Position(2, 2), StandStillSpeed);
        world.Add(animal);

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        animal.Health.Should().Be(Health);
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Tick_WhenHealthRunsOut_ShouldRemoveAnimal(AnimalKind kind)
    {
        // Arrange
        double healthForOneStep = 0.5;

        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAnimal(kind, new Position(2, 2), OneStepSpeed, healthForOneStep));

        GameEngine engine = CreateEngine(world, new GameOptions { MoveHealthCost = 0.5 });

        // Act
        engine.Tick(Tick);

        // Assert
        world.Snapshot().Animals.Should().BeEmpty();
    }

    [Fact]
    public void Tick_WhenLionEatsAntelope_ShouldGainHealth()
    {
        // Arrange
        double damage = 15;
        double healthGain = 10;
        double expectedHealth = Health - damage + healthGain;

        var sharedCell = new Position(2, 0);
        GameWorld world = CreateWorld(5, 1);
        Animal lion = CreateAnimal(AnimalKind.Lion, sharedCell, StandStillSpeed);
        lion.LoseHealth(damage);
        world.Add(lion);
        world.Add(CreateAnimal(AnimalKind.Antelope, sharedCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world, new GameOptions { LionHealthGainOnEat = healthGain });

        // Act
        engine.Tick(Tick);

        // Assert
        lion.Health.Should().Be(expectedHealth);
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Tick_WhenSameKindStayNearForRequiredRounds_ShouldGiveBirth(AnimalKind kind)
    {
        // Arrange
        int roundsRequired = 3;
        int expectedCount = 3;

        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAnimal(kind, new Position(2, 2), StandStillSpeed));
        world.Add(CreateAnimal(kind, new Position(3, 2), StandStillSpeed));

        GameEngine engine = CreateEngine(world, OneRoundPerTick(roundsRequired));

        // Act
        for (int i = 0; i < roundsRequired; i++)
            engine.Tick(Tick);

        // Assert
        var animals = world.Snapshot().Animals;

        animals.Should().HaveCount(expectedCount);
        animals.Should().OnlyContain(animal => animal.Kind == kind);
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Tick_WhenSameKindNearForFewerRounds_ShouldNotGiveBirth(AnimalKind kind)
    {
        // Arrange
        int roundsRequired = 3;
        int expectedCount = 2;

        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAnimal(kind, new Position(2, 2), StandStillSpeed));
        world.Add(CreateAnimal(kind, new Position(3, 2), StandStillSpeed));

        GameEngine engine = CreateEngine(world, OneRoundPerTick(roundsRequired));

        // Act
        for (int i = 0; i < roundsRequired - 1; i++)
            engine.Tick(Tick);

        // Assert
        world.Snapshot().Animals.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void Tick_WhenDifferentKindsStayNear_ShouldNotGiveBirth()
    {
        // Arrange
        int roundsRequired = 3;
        int expectedCount = 2;

        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAnimal(AnimalKind.Lion, new Position(2, 2), StandStillSpeed));
        world.Add(CreateAnimal(AnimalKind.Antelope, new Position(3, 2), StandStillSpeed));

        GameEngine engine = CreateEngine(world, OneRoundPerTick(roundsRequired));

        // Act
        for (int i = 0; i < roundsRequired; i++)
            engine.Tick(Tick);

        // Assert
        world.Snapshot().Animals.Should().HaveCount(expectedCount);
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Tick_WhenBirthHappens_ShouldPlaceNewbornNextToParent(AnimalKind kind)
    {
        // Arrange
        int roundsRequired = 3;

        var firstParent = new Position(0, 0);
        var secondParent = new Position(1, 0);

        GameWorld world = CreateWorld(10, 10);
        world.Add(CreateAnimal(kind, firstParent, StandStillSpeed));
        world.Add(CreateAnimal(kind, secondParent, StandStillSpeed));

        GameEngine engine = CreateEngine(world, OneRoundPerTick(roundsRequired));

        // Act
        for (int i = 0; i < roundsRequired; i++)
            engine.Tick(Tick);

        // Assert
        Position newborn = world.Snapshot().Animals
            .Select(animal => animal.Position)
            .Single(position => position != firstParent && position != secondParent);

        Math.Min(newborn.DistanceTo(firstParent), newborn.DistanceTo(secondParent)).Should().Be(1);
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Tick_WhenNoFreeCellNextToParents_ShouldNotGiveBirth(AnimalKind kind)
    {
        // Arrange
        int roundsRequired = 3;
        int expectedCount = 2;

        GameWorld world = CreateWorld(2, 1);
        world.Add(CreateAnimal(kind, new Position(0, 0), StandStillSpeed));
        world.Add(CreateAnimal(kind, new Position(1, 0), StandStillSpeed));

        GameEngine engine = CreateEngine(world, OneRoundPerTick(roundsRequired));

        // Act
        for (int i = 0; i < roundsRequired; i++)
            engine.Tick(Tick);

        // Assert
        world.Snapshot().Animals.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void Tick_WhenRoundIsLongerThanTick_ShouldWaitForFullRounds()
    {
        // Arrange
        int ticksPerRound = 10;
        int roundsRequired = 3;
        int ticksBeforeBirth = ticksPerRound * roundsRequired - 1;

        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAnimal(AnimalKind.Antelope, new Position(2, 2), StandStillSpeed));
        world.Add(CreateAnimal(AnimalKind.Antelope, new Position(3, 2), StandStillSpeed));

        var options = new GameOptions { RoundMilliseconds = 1000, BirthRoundsRequired = roundsRequired };
        GameEngine engine = CreateEngine(world, options);

        // Act
        for (int i = 0; i < ticksBeforeBirth; i++)
            engine.Tick(Tick);

        int countBeforeLastTick = world.Snapshot().Animals.Count;

        engine.Tick(Tick);

        int countAfterLastTick = world.Snapshot().Animals.Count;

        // Assert
        countBeforeLastTick.Should().Be(2);
        countAfterLastTick.Should().Be(3);
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Tick_WhenNextCellIsOccupied_ShouldNotMoveOrLoseHealth(AnimalKind kind)
    {
        // Arrange
        var start = new Position(0, 0);
        var occupiedCell = new Position(1, 0);

        GameWorld world = CreateWorld(2, 1);
        Animal mover = CreateAnimal(kind, start, OneStepSpeed);
        world.Add(mover);
        world.Add(CreateAnimal(kind, occupiedCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        mover.Position.Should().Be(start);
        mover.Health.Should().Be(Health);
    }

    [Fact]
    public void Tick_WhenLionStepsOntoAntelope_ShouldEatIt()
    {
        // Arrange
        int expectedCount = 1;
        var antelopeCell = new Position(1, 0);

        GameWorld world = CreateWorld(5, 1);
        Animal lion = CreateAnimal(AnimalKind.Lion, new Position(0, 0), OneStepSpeed);
        world.Add(lion);
        world.Add(CreateAnimal(AnimalKind.Antelope, antelopeCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        lion.Position.Should().Be(antelopeCell);
        world.Snapshot().Animals.Should().HaveCount(expectedCount);
    }

    // Initialize options where every tick completes one round, so birth tests need few ticks
    private static GameOptions OneRoundPerTick(int roundsRequired)
    {
        return new GameOptions
        {
            RoundMilliseconds = (int)Tick.TotalMilliseconds,
            BirthRoundsRequired = roundsRequired
        };
    }

    // Initialize a world of the given size
    private static GameWorld CreateWorld(int width, int height)
    {
        var options = new GameOptions { FieldWidth = width, FieldHeight = height };

        return new GameWorld(options);
    }

    // Initialize an engine with the real factories and services, all sharing the same world
    private static GameEngine CreateEngine(GameWorld world, GameOptions? options = null)
    {
        options ??= new GameOptions();

        var random = new Random();
        var factory = new AnimalFactory(new AnimalFactoryBase[]
        {
        new AntelopeFactory(options, random),
        new LionFactory(options, random)
        });

        var movement = new AnimalMovement(world, options);
        var birth = new AnimalBirth(world, factory, random, options);

        return new GameEngine(world, factory, random, options, movement, birth);
    }

    // Initialize an animal of the given kind with the given position and speed
    private static Animal CreateAnimal(AnimalKind kind, Position position, double speed, double health = Health)
    {
        var random = new Random();

        return kind switch
        {
            AnimalKind.Lion => new Lion(position, speed, Vision, health, new LionBehavior(random)),
            AnimalKind.Antelope => new Antelope(position, speed, Vision, health, new AntelopeBehavior(random)),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "No test animal for this kind.")
        };
    }
}