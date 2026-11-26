using FluentAssertions;
using Moq;
using Savanna.AnimalsBase.Factories;
using Savanna.AnimalsBase.Interfaces;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Configuration;
using Savanna.Core.Interfaces;
using Savanna.Core.Models;
using Savanna.Core.Services;
using xUnit.Tests.Savanna.TestDoubles;

namespace xUnit.Tests.Savanna;

// Tests the engine rules only. Species and behaviors are test doubles, so no real animal is involved:
// chasing and fleeing belong in the behavior tests.
[Trait("Category", "GameLogic")]
public class GameEngineTests
{
    private const double StandStillSpeed = 0.001;   // never builds a full step during these tests
    private const double OneStepSpeed = 10;         // 1.0 credit per tick, exactly one step per tick
    private const double Health = TestSpecies.DefaultHealth;

    private static readonly TimeSpan Tick = TimeSpan.FromMilliseconds(100);

    [Fact]
    public void TryAddAnimal_WhenFieldHasSpace_ShouldReturnTrueAndAddAnimal()
    {
        // Arrange
        int expectedCount = 1;
        SpeciesId species = TestSpecies.PreyId;

        var random = new Random();
        var options = new GameOptions();
        GameWorld world = CreateWorld(5, 5);

        var factoryMock = new Mock<IAnimalFactory>();
        factoryMock
            .Setup(factory => factory.Create(species, It.IsAny<Position>()))
            .Returns((SpeciesId _, Position position) => CreateAnimal(Role.Prey, position, StandStillSpeed));

        var movement = new AnimalMovement(world, options);
        var birth = new AnimalBirth(world, factoryMock.Object, random, options);
        var engine = new GameEngine(world, factoryMock.Object, random, options, movement, birth);

        // Act
        bool added = engine.TryAddAnimal(species);

        // Assert
        added.Should().BeTrue();
        world.Snapshot().Animals.Should().HaveCount(expectedCount);
        world.Snapshot().Animals[0].Species.Should().Be(species);
        factoryMock.Verify(factory => factory.Create(species, It.IsAny<Position>()), Times.Once);
    }

    [Fact]
    public void TryAddAnimal_WhenFieldIsFull_ShouldReturnFalse()
    {
        // Arrange
        int expectedCount = 1;

        GameWorld world = CreateWorld(1, 1);
        GameEngine engine = CreateEngine(world);

        engine.TryAddAnimal(TestSpecies.PreyId);

        // Act
        bool added = engine.TryAddAnimal(TestSpecies.PredatorId);

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
            engine.TryAddAnimal(TestSpecies.PreyId);

        // Assert
        var positions = world.Snapshot().Animals.Select(animal => animal.Position).ToList();

        positions.Should().HaveCount(cellCount);
        positions.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Tick_WhenCreditIsBelowOne_ShouldNotMoveAnimal()
    {
        // Arrange
        double speed = 4;    // 0.4 credit per tick, 0.8 after two ticks
        int ticks = 2;

        var start = new Position(2, 2);
        GameWorld world = CreateWorld(5, 5);
        Animal animal = CreateAnimal(Role.Prey, start, speed, StubBehavior.Step(StubBehavior.Right));
        world.Add(animal);

        GameEngine engine = CreateEngine(world);

        // Act
        for (int i = 0; i < ticks; i++)
            engine.Tick(Tick);

        // Assert
        animal.Position.Should().Be(start);
    }

    [Fact]
    public void Tick_WhenCreditReachesOne_ShouldMoveExactlyOneCell()
    {
        // Arrange
        double speed = 4;    // 1.2 credit on the third tick
        int ticks = 3;

        var start = new Position(2, 2);
        var expected = start + StubBehavior.Right;

        GameWorld world = CreateWorld(5, 5);
        Animal animal = CreateAnimal(Role.Prey, start, speed, StubBehavior.Step(StubBehavior.Right));
        world.Add(animal);

        GameEngine engine = CreateEngine(world);

        // Act
        for (int i = 0; i < ticks; i++)
            engine.Tick(Tick);

        // Assert
        animal.Position.Should().Be(expected);
    }

    [Fact]
    public void Tick_WhenStepWouldLeaveField_ShouldStayInsideAndKeepHealth()
    {
        // Arrange
        var corner = new Position(0, 0);

        GameWorld world = CreateWorld(3, 3);
        Animal animal = CreateAnimal(Role.Prey, corner, OneStepSpeed, StubBehavior.Step(StubBehavior.Left));
        world.Add(animal);

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        animal.Position.Should().Be(corner);
        animal.Health.Should().Be(Health);
    }

    [Theory]
    [InlineData(Role.Prey)]
    [InlineData(Role.Predator)]
    public void Tick_WhenNextCellHasSameSpecies_ShouldNotMoveOrLoseHealth(Role role)
    {
        // Arrange
        var start = new Position(0, 0);
        var occupiedCell = new Position(1, 0);

        GameWorld world = CreateWorld(2, 1);
        Animal mover = CreateAnimal(role, start, OneStepSpeed, StubBehavior.Step(StubBehavior.Right));
        world.Add(mover);
        world.Add(CreateAnimal(role, occupiedCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        mover.Position.Should().Be(start);
        mover.Health.Should().Be(Health);
    }

    [Fact]
    public void Tick_WhenPreyStepsTowardPredator_ShouldBeBlocked()
    {
        // Arrange
        var start = new Position(0, 0);

        GameWorld world = CreateWorld(2, 1);
        Animal prey = CreateAnimal(Role.Prey, start, OneStepSpeed, StubBehavior.Step(StubBehavior.Right));
        world.Add(prey);
        world.Add(CreateAnimal(Role.Predator, new Position(1, 0), StandStillSpeed));

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        prey.Position.Should().Be(start);
        world.Snapshot().Animals.Should().HaveCount(2);
    }

    [Fact]
    public void Tick_WhenAnimalMovesOneCell_ShouldLoseMoveHealthCost()
    {
        // Arrange
        double moveCost = 0.5;
        double expectedHealth = Health - moveCost;

        GameWorld world = CreateWorld(5, 5);
        Animal animal = CreateAnimal(Role.Prey, new Position(2, 2), OneStepSpeed, StubBehavior.Step(StubBehavior.Right));
        world.Add(animal);

        GameEngine engine = CreateEngine(world, new GameOptions { MoveHealthCost = moveCost });

        // Act
        engine.Tick(Tick);

        // Assert
        animal.Health.Should().Be(expectedHealth);
    }

    [Fact]
    public void Tick_WhenAnimalDoesNotMove_ShouldKeepHealth()
    {
        // Arrange
        GameWorld world = CreateWorld(5, 5);
        Animal animal = CreateAnimal(Role.Prey, new Position(2, 2), OneStepSpeed, StubBehavior.StandStill());
        world.Add(animal);

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        animal.Health.Should().Be(Health);
    }

    [Fact]
    public void Tick_WhenHealthRunsOut_ShouldRemoveAnimal()
    {
        // Arrange
        double moveCost = 0.5;
        double healthForOneStep = moveCost;

        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAnimal(Role.Prey, new Position(2, 2), OneStepSpeed, StubBehavior.Step(StubBehavior.Right), healthForOneStep));

        GameEngine engine = CreateEngine(world, new GameOptions { MoveHealthCost = moveCost });

        // Act
        engine.Tick(Tick);

        // Assert
        world.Snapshot().Animals.Should().BeEmpty();
    }

    [Fact]
    public void Tick_WhenPredatorSharesCellWithPrey_ShouldRemovePrey()
    {
        // Arrange
        int expectedCount = 1;

        var sharedCell = new Position(2, 0);
        GameWorld world = CreateWorld(5, 1);
        world.Add(CreateAnimal(Role.Predator, sharedCell, StandStillSpeed));
        world.Add(CreateAnimal(Role.Prey, sharedCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        var animals = world.Snapshot().Animals;

        animals.Should().HaveCount(expectedCount);
        animals[0].Species.Should().Be(TestSpecies.PredatorId);
    }

    [Theory]
    [InlineData(Role.Prey)]
    [InlineData(Role.Predator)]
    public void Tick_WhenSameSpeciesShareCell_ShouldNotEatEachOther(Role role)
    {
        // Arrange
        int expectedCount = 2;

        var sharedCell = new Position(2, 0);
        GameWorld world = CreateWorld(5, 1);
        world.Add(CreateAnimal(role, sharedCell, StandStillSpeed));
        world.Add(CreateAnimal(role, sharedCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        world.Snapshot().Animals.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void Tick_WhenPredatorTargetsOneOfSeveralPrey_ShouldEatTheTarget()
    {
        // Arrange
        var sharedCell = new Position(2, 0);
        GameWorld world = CreateWorld(5, 1);

        Animal firstPrey = CreateAnimal(Role.Prey, sharedCell, StandStillSpeed);
        Animal targetPrey = CreateAnimal(Role.Prey, sharedCell, StandStillSpeed);
        Animal predator = CreateAnimal(Role.Predator, sharedCell, StandStillSpeed, StubBehavior.Target(targetPrey.Id));

        world.Add(firstPrey);
        world.Add(targetPrey);
        world.Add(predator);

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        var remainingIds = world.Snapshot().Animals.Select(animal => animal.Id).ToList();

        remainingIds.Should().Contain(firstPrey.Id);
        remainingIds.Should().NotContain(targetPrey.Id);
    }

    [Fact]
    public void Tick_WhenPredatorEats_ShouldGainHealth()
    {
        // Arrange
        double damage = 15;
        double healthGain = 10;
        double expectedHealth = Health - damage + healthGain;

        var sharedCell = new Position(2, 0);
        GameWorld world = CreateWorld(5, 1);
        Animal predator = CreateAnimal(Role.Predator, sharedCell, StandStillSpeed);
        predator.LoseHealth(damage);
        world.Add(predator);
        world.Add(CreateAnimal(Role.Prey, sharedCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world, new GameOptions { HealthGainOnEat = healthGain });

        // Act
        engine.Tick(Tick);

        // Assert
        predator.Health.Should().Be(expectedHealth);
    }

    [Fact]
    public void Tick_WhenNewPredatorSpeciesHuntsSamePrey_ShouldEatIt()
    {
        // Arrange
        int expectedCount = 1;
        var importedId = new SpeciesId("TestImportedPredator");
        var imported = new SpeciesDescriptor(
            importedId, "Imported", "II", 'I', StandStillSpeed, TestSpecies.DefaultVision, Health,
            new HashSet<SpeciesId> { TestSpecies.PreyId });

        var sharedCell = new Position(2, 0);
        GameWorld world = CreateWorld(5, 1, new StubFactory(imported));
        world.Add(new Animal(imported, sharedCell, StubBehavior.StandStill()));
        world.Add(CreateAnimal(Role.Prey, sharedCell, StandStillSpeed));

        GameEngine engine = CreateEngine(world);

        // Act
        engine.Tick(Tick);

        // Assert
        var animals = world.Snapshot().Animals;

        animals.Should().HaveCount(expectedCount);
        animals[0].Species.Should().Be(importedId);
    }

    [Theory]
    [InlineData(Role.Prey)]
    [InlineData(Role.Predator)]
    public void Tick_WhenSameSpeciesStayNearForRequiredRounds_ShouldGiveBirth(Role role)
    {
        // Arrange
        int roundsRequired = 3;
        int expectedCount = 3;
        SpeciesId species = TestSpecies.IdFor(role);

        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAnimal(role, new Position(2, 2), StandStillSpeed));
        world.Add(CreateAnimal(role, new Position(3, 2), StandStillSpeed));

        GameEngine engine = CreateEngine(world, OneRoundPerTick(roundsRequired));

        // Act
        for (int i = 0; i < roundsRequired; i++)
            engine.Tick(Tick);

        // Assert
        var animals = world.Snapshot().Animals;

        animals.Should().HaveCount(expectedCount);
        animals.Should().OnlyContain(animal => animal.Species == species);
    }

    [Theory]
    [InlineData(Role.Prey)]
    [InlineData(Role.Predator)]
    public void Tick_WhenSameSpeciesNearForFewerRounds_ShouldNotGiveBirth(Role role)
    {
        // Arrange
        int roundsRequired = 3;
        int expectedCount = 2;

        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAnimal(role, new Position(2, 2), StandStillSpeed));
        world.Add(CreateAnimal(role, new Position(3, 2), StandStillSpeed));

        GameEngine engine = CreateEngine(world, OneRoundPerTick(roundsRequired));

        // Act
        for (int i = 0; i < roundsRequired - 1; i++)
            engine.Tick(Tick);

        // Assert
        world.Snapshot().Animals.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void Tick_WhenDifferentSpeciesStayNear_ShouldNotGiveBirth()
    {
        // Arrange
        int roundsRequired = 3;
        int expectedCount = 2;

        GameWorld world = CreateWorld(5, 5);
        world.Add(CreateAnimal(Role.Predator, new Position(2, 2), StandStillSpeed));
        world.Add(CreateAnimal(Role.Prey, new Position(3, 2), StandStillSpeed));

        GameEngine engine = CreateEngine(world, OneRoundPerTick(roundsRequired));

        // Act
        for (int i = 0; i < roundsRequired; i++)
            engine.Tick(Tick);

        // Assert
        world.Snapshot().Animals.Should().HaveCount(expectedCount);
    }

    [Theory]
    [InlineData(Role.Prey)]
    [InlineData(Role.Predator)]
    public void Tick_WhenBirthHappens_ShouldPlaceNewbornNextToParent(Role role)
    {
        // Arrange
        int roundsRequired = 3;

        var firstParent = new Position(0, 0);
        var secondParent = new Position(1, 0);

        GameWorld world = CreateWorld(10, 10);
        world.Add(CreateAnimal(role, firstParent, StandStillSpeed));
        world.Add(CreateAnimal(role, secondParent, StandStillSpeed));

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
    [InlineData(Role.Prey)]
    [InlineData(Role.Predator)]
    public void Tick_WhenNoFreeCellNextToParents_ShouldNotGiveBirth(Role role)
    {
        // Arrange
        int roundsRequired = 3;
        int expectedCount = 2;

        GameWorld world = CreateWorld(2, 1);
        world.Add(CreateAnimal(role, new Position(0, 0), StandStillSpeed));
        world.Add(CreateAnimal(role, new Position(1, 0), StandStillSpeed));

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
        world.Add(CreateAnimal(Role.Prey, new Position(2, 2), StandStillSpeed));
        world.Add(CreateAnimal(Role.Prey, new Position(3, 2), StandStillSpeed));

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

    // Initialize options where every tick completes one round, so birth tests need few ticks
    private static GameOptions OneRoundPerTick(int roundsRequired)
    {
        return new GameOptions
        {
            RoundMilliseconds = (int)Tick.TotalMilliseconds,
            BirthRoundsRequired = roundsRequired
        };
    }

    // The test species factories; extra factories add more species to the world's registry
    private static AnimalFactoryBase[] CreateFactories(params AnimalFactoryBase[] extra) =>
    [
        new StubFactory(TestSpecies.Prey()),
        new StubFactory(TestSpecies.Predator()),
        .. extra
    ];

    // Initialize a world of the given size that knows the test species
    private static GameWorld CreateWorld(int width, int height, params AnimalFactoryBase[] extraSpecies)
    {
        var options = new GameOptions { FieldWidth = width, FieldHeight = height };

        return new GameWorld(options, new SpeciesRegistry(CreateFactories(extraSpecies)));
    }

    // Initialize an engine with the real engine services and the test species factories, all sharing the same world
    private static GameEngine CreateEngine(GameWorld world, GameOptions? options = null)
    {
        options ??= new GameOptions();

        var random = new Random();
        var factory = new AnimalFactory(CreateFactories());

        var movement = new AnimalMovement(world, options);
        var birth = new AnimalBirth(world, factory, random, options);

        return new GameEngine(world, factory, random, options, movement, birth);
    }

    // Initialize an animal that plays the given role; it stands still unless a behavior is given
    private static Animal CreateAnimal(Role role, Position position, double speed, IAnimalBehavior? behavior = null, double health = Health)
    {
        SpeciesDescriptor species = TestSpecies.For(role, speed, TestSpecies.DefaultVision, health);

        return new Animal(species, position, behavior ?? StubBehavior.StandStill());
    }
}
