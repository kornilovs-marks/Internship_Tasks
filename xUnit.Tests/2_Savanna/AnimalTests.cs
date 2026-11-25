using FluentAssertions;
using Savanna.Core.Behaviors;
using Savanna.Core.Models;

namespace xUnit.Tests.Savanna;

[Trait("Category", "Models")]
public class AnimalTests
{
    private const double ValidSpeed = 5;
    private const int ValidVisionRange = 9;
    private const double ValidHealth = 20;

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Constructor_ShouldSetKindAndProperties(AnimalKind kind)
    {
        // Arrange
        double speed = 4;
        int visionRange = 7;
        double health = 12;

        var position = new Position(3, 4);

        // Act
        Animal animal = CreateAnimal(kind, position, speed, visionRange, health);

        // Assert
        animal.Kind.Should().Be(kind);
        animal.Position.Should().Be(position);
        animal.Speed.Should().Be(speed);
        animal.VisionRange.Should().Be(visionRange);
        animal.Health.Should().Be(health);
        animal.Behavior.Should().BeOfType(ExpectedBehaviorType(kind));
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Constructor_WhenCreatedTwice_ShouldAssignUniqueIds(AnimalKind kind)
    {
        // Arrange / Act
        Animal first = CreateAnimal(kind);
        Animal second = CreateAnimal(kind);

        // Assert
        first.Id.Should().NotBe(second.Id);
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void Constructor_WhenCreated_ShouldStartAliveWithFullHealth(AnimalKind kind)
    {
        // Arrange / Act
        Animal animal = CreateAnimal(kind);

        // Assert
        animal.Health.Should().Be(ValidHealth);
        animal.MaxHealth.Should().Be(ValidHealth);
        animal.IsAlive.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Constructor_WhenSpeedIsInvalid_ShouldThrowArgumentException(double invalidSpeed)
    {
        // Arrange / Act
        Action act = () => CreateAnimal(AnimalKind.Lion, speed: invalidSpeed);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Constructor_WhenHealthIsInvalid_ShouldThrowArgumentException(double invalidHealth)
    {
        // Arrange / Act
        Action act = () => CreateAnimal(AnimalKind.Lion, health: invalidHealth);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenVisionRangeIsNegative_ShouldThrowArgumentException()
    {
        // Arrange / Act
        Action act = () => CreateAnimal(AnimalKind.Antelope, visionRange: -1);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void LoseHealth_ShouldReduceHealthByAmount(AnimalKind kind)
    {
        // Arrange
        double moveCost = 0.5;
        double expectedHealth = ValidHealth - moveCost;

        Animal animal = CreateAnimal(kind);

        // Act
        animal.LoseHealth(moveCost);

        // Assert
        animal.Health.Should().Be(expectedHealth);
        animal.IsAlive.Should().BeTrue();
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void LoseHealth_WhenAmountIsMoreThanHealth_ShouldStopAtZeroAndDie(AnimalKind kind)
    {
        // Arrange
        double expectedHealth = 0;

        Animal animal = CreateAnimal(kind);

        // Act
        animal.LoseHealth(ValidHealth + 5);

        // Assert
        animal.Health.Should().Be(expectedHealth);
        animal.IsAlive.Should().BeFalse();
    }
    [Fact]
    public void Heal_ShouldIncreaseHealthByAmount()
    {
        // Arrange
        double damage = 10;
        double healing = 4;
        double expectedHealth = ValidHealth - damage + healing;

        Animal lion = CreateAnimal(AnimalKind.Lion);
        lion.LoseHealth(damage);

        // Act
        lion.Heal(healing);

        // Assert
        lion.Health.Should().Be(expectedHealth);
    }

    [Fact]
    public void Heal_WhenAmountWouldExceedMaxHealth_ShouldStopAtMaxHealth()
    {
        // Arrange
        double damage = 2;
        double healing = 10;

        Animal lion = CreateAnimal(AnimalKind.Lion);
        lion.LoseHealth(damage);

        // Act
        lion.Heal(healing);

        // Assert
        lion.Health.Should().Be(lion.MaxHealth);
    }

    [Fact]
    public void LoseHealth_WhenAmountIsNegative_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        Animal animal = CreateAnimal(AnimalKind.Antelope);

        // Act
        Action act = () => animal.LoseHealth(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Heal_WhenAmountIsNegative_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        Animal animal = CreateAnimal(AnimalKind.Lion);

        // Act
        Action act = () => animal.Heal(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(AnimalKind.Lion)]
    [InlineData(AnimalKind.Antelope)]
    public void ToView_ShouldCopyCurrentValues(AnimalKind kind)
    {
        // Arrange
        Animal animal = CreateAnimal(kind);

        // Act
        AnimalView view = animal.ToView();

        // Assert
        view.Id.Should().Be(animal.Id);
        view.Kind.Should().Be(animal.Kind);
        view.Position.Should().Be(animal.Position);
        view.Speed.Should().Be(animal.Speed);
        view.VisionRange.Should().Be(animal.VisionRange);
        view.Health.Should().Be(animal.Health);
    }

    [Fact]
    public void ToView_WhenAnimalChangesAfterwards_ShouldKeepOldValuesInEarlierView()
    {
        // Arrange
        double moveCost = 0.5;
        var startPosition = new Position(2, 2);
        var newPosition = new Position(3, 2);

        Animal animal = CreateAnimal(AnimalKind.Antelope, startPosition);
        AnimalView viewBeforeMove = animal.ToView();

        // Act
        animal.Position = newPosition;
        animal.LoseHealth(moveCost);
        AnimalView viewAfterMove = animal.ToView();

        // Assert
        viewBeforeMove.Position.Should().Be(startPosition);
        viewBeforeMove.Health.Should().Be(ValidHealth);
        viewAfterMove.Position.Should().Be(newPosition);
        viewAfterMove.Health.Should().Be(ValidHealth - moveCost);
    }

    // Initialize an animal of the given kind; each test only passes the values it cares about
    private static Animal CreateAnimal(
        AnimalKind kind,
        Position position = default,
        double speed = ValidSpeed,
        int visionRange = ValidVisionRange,
        double health = ValidHealth)
    {
        var random = new Random();

        return kind switch
        {
            AnimalKind.Lion => new Lion(position, speed, visionRange, health, new LionBehavior(random)),
            AnimalKind.Antelope => new Antelope(position, speed, visionRange, health, new AntelopeBehavior(random)),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "No test animal for this kind.")
        };
    }

    // The behavior class each animal kind must be created with
    private static Type ExpectedBehaviorType(AnimalKind kind) =>
        kind == AnimalKind.Lion ? typeof(LionBehavior) : typeof(AntelopeBehavior);
}