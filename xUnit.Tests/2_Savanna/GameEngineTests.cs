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

        var engine = new GameEngine(world, factoryMock.Object, random, new GameOptions());

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
        double enoughHealthForAllSteps = 1000;

        GameWorld world = CreateWorld(3, 3);
        world.Add(CreateAntelope(new Position(0, 0), OneStepSpeed, enoughHealthForAllSteps));

        GameEngine engine = CreateEngine(world);

        for (int i = 0; i < ticks; i++)
        {
            // Act
            engine.Tick(Tick);

            // Assert
            world.Contains(world.Snapshot().Animals[0].Position).Should().BeTrue();
        }
    }

    [Fact]
    public void Tick_WhenAnimalMovesOneCell_ShouldLoseMoveHealthCost()
    {
        // Arrange
        double moveCost = 0.5;
        double expectedHealth = Health - moveCost;

        GameWorld world = CreateWorld(5, 5);
        Antelope antelope = CreateAntelope(new Position(2, 2), OneStepSpeed);
        world.Add(antelope);

        GameEngine engine = CreateEngine(world, new GameOptions { MoveHealthCost = moveCost });

        // Act
        engine.Tick(Tick);

        // Assert
        antelope.Health.Should().Be(expectedHealth);
    }

    [Fact]
    public void Tick_WhenAnimalDoesNotMove_ShouldKeepHealth()
    {
        // Arrange
        GameWorld world = CreateWorld(5, 5);
        Antelope antelope = CreateAntelope(new Position(2, 2), StandStillSpeed);
        world.Add(antelope);

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        antelope.Health.Should().Be(Health);
    }

    [Fact]
    public void Tick_WhenHealthRunsOut_ShouldRemoveAnimal()
    {
        // Arrange
        double healthForOneStep = 0.5;

        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAntelope(new Position(2, 2), OneStepSpeed, healthForOneStep));

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
        Lion lion = CreateLion(sharedCell, StandStillSpeed);
        lion.LoseHealth(damage);
        world.Add(lion);
        world.Add(CreateAntelope(sharedCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world, new GameOptions { LionHealthGainOnEat = healthGain });

        // Act
        engine.Tick(Tick);

        // Assert
        lion.Health.Should().Be(expectedHealth);
    }

    [Fact]
    public void Tick_WhenLionEatsWithNearlyFullHealth_ShouldNotExceedMaxHealth()
    {
        // Arrange
        var sharedCell = new Position(2, 0);
        GameWorld world = CreateWorld(5, 1);
        Lion lion = CreateLion(sharedCell, StandStillSpeed);
        lion.LoseHealth(1);
        world.Add(lion);
        world.Add(CreateAntelope(sharedCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world, new GameOptions { LionHealthGainOnEat = 10 });

        // Act
        engine.Tick(Tick);

        // Assert
        lion.Health.Should().Be(lion.MaxHealth);
    }

    [Fact]
    public void Tick_WhenSameKindStayNearForRequiredRounds_ShouldGiveBirth()
    {
        // Arrange
        int roundsRequired = 3;
        int expectedCount = 3;

        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAntelope(new Position(2, 2), StandStillSpeed));
        world.Add(CreateAntelope(new Position(3, 2), StandStillSpeed));

        GameEngine engine = CreateEngine(world, OneRoundPerTick(roundsRequired));

        // Act
        for (int i = 0; i < roundsRequired; i++)
            engine.Tick(Tick);

        // Assert
        var animals = world.Snapshot().Animals;

        animals.Should().HaveCount(expectedCount);
        animals.Should().OnlyContain(animal => animal.Kind == AnimalKind.Antelope);
    }

    [Fact]
    public void Tick_WhenSameKindNearForFewerRounds_ShouldNotGiveBirth()
    {
        // Arrange
        int roundsRequired = 3;
        int expectedCount = 2;

        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAntelope(new Position(2, 2), StandStillSpeed));
        world.Add(CreateAntelope(new Position(3, 2), StandStillSpeed));

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
        world.Add(CreateLion(new Position(2, 2), StandStillSpeed));
        world.Add(CreateAntelope(new Position(3, 2), StandStillSpeed));

        GameEngine engine = CreateEngine(world, OneRoundPerTick(roundsRequired));

        // Act
        for (int i = 0; i < roundsRequired; i++)
            engine.Tick(Tick);

        // Assert
        world.Snapshot().Animals.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void Tick_WhenBirthHappens_ShouldPlaceNewbornNextToParent()
    {
        // Arrange
        int roundsRequired = 3;

        var firstParent = new Position(0, 0);
        var secondParent = new Position(1, 0);

        GameWorld world = CreateWorld(10, 10);
        world.Add(CreateLion(firstParent, StandStillSpeed));
        world.Add(CreateLion(secondParent, StandStillSpeed));

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

    [Fact]
    public void Tick_WhenNoFreeCellNextToParents_ShouldNotGiveBirth()
    {
        // Arrange
        int roundsRequired = 3;
        int expectedCount = 2;

        GameWorld world = CreateWorld(2, 1);    // the two parents fill the whole field
        world.Add(CreateAntelope(new Position(0, 0), StandStillSpeed));
        world.Add(CreateAntelope(new Position(1, 0), StandStillSpeed));

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
        int ticksPerRound = 10;                   // 1000 ms round, 100 ms tick
        int roundsRequired = 3;
        int ticksBeforeBirth = ticksPerRound * roundsRequired - 1;

        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAntelope(new Position(2, 2), StandStillSpeed));
        world.Add(CreateAntelope(new Position(3, 2), StandStillSpeed));

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

    [Fact]
    public void Tick_WhenNextCellIsOccupied_ShouldNotMoveOrLoseHealth()
    {
        // Arrange
        var start = new Position(0, 0);
        var occupiedCell = new Position(1, 0);

        GameWorld world = CreateWorld(2, 1);    // the only way to go is onto the other antelope
        Antelope mover = CreateAntelope(start, OneStepSpeed);
        world.Add(mover);
        world.Add(CreateAntelope(occupiedCell, StandStillSpeed));

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
        Lion lion = CreateLion(new Position(0, 0), OneStepSpeed);
        world.Add(lion);
        world.Add(CreateAntelope(antelopeCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        lion.Position.Should().Be(antelopeCell);
        world.Snapshot().Animals.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void Tick_WhenAnotherLionBlocksTheWay_ShouldNotMove()
    {
        // Arrange
        var start = new Position(0, 0);

        GameWorld world = CreateWorld(5, 1);
        Lion hunter = CreateLion(start, OneStepSpeed);
        world.Add(hunter);
        world.Add(CreateLion(new Position(1, 0), StandStillSpeed));
        world.Add(CreateAntelope(new Position(2, 0), StandStillSpeed));

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        hunter.Position.Should().Be(start);
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

    // Initialize an engine with the real factories
    private static GameEngine CreateEngine(GameWorld world, GameOptions? options = null)
    {
        options ??= new GameOptions();

        var random = new Random();
        var factories = new AnimalFactoryBase[]
        {
            new AntelopeFactory(options, random),
            new LionFactory(options, random)
        };

        return new GameEngine(world, new AnimalFactory(factories), random, options);
    }

    // Initialize a lion with the given position and speed
    private static Lion CreateLion(Position position, double speed, double health = Health)
    {
        return new Lion(position, speed, Vision, health, new LionBehavior(new Random()));
    }

    // Initialize an antelope with the given position and speed
    private static Antelope CreateAntelope(Position position, double speed, double health = Health)
    {
        return new Antelope(position, speed, Vision, health, new AntelopeBehavior(new Random()));
    }
}