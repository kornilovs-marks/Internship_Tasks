using FluentAssertions;
using Savanna.AnimalsBase.Interfaces;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Models;
using xUnit.Tests.Savanna.TestDoubles;

namespace xUnit.Tests.Savanna;

[Trait("Category", "Models")]
public class AnimalTests
{
    private const double ValidSpeed = 5;
    private const int ValidVisionRange = 9;
    private const double ValidHealth = 20;

    [Theory]
    [InlineData(Role.Prey)]
    [InlineData(Role.Predator)]
    public void Constructor_ShouldSetSpeciesAndProperties(Role role)
    {
        // Arrange
        double speed = 4;
        int visionRange = 7;
        double health = 12;

        var position = new Position(3, 4);
        SpeciesDescriptor species = TestSpecies.For(role, speed, visionRange, health);
        IAnimalBehavior behavior = StubBehavior.StandStill();

        // Act
        var animal = new Animal(species, position, behavior);

        // Assert
        animal.Species.Should().BeSameAs(species);
        animal.SpeciesId.Should().Be(TestSpecies.IdFor(role));
        animal.Position.Should().Be(position);
        animal.Speed.Should().Be(speed);
        animal.VisionRange.Should().Be(visionRange);
        animal.Health.Should().Be(health);
        animal.Behavior.Should().BeSameAs(behavior);
    }

    [Fact]
    public void Constructor_WhenCreatedTwice_ShouldAssignUniqueIds()
    {
        // Arrange / Act
        Animal first = CreateAnimal(Role.Prey);
        Animal second = CreateAnimal(Role.Prey);

        // Assert
        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public void Constructor_WhenCreated_ShouldStartAliveWithFullHealth()
    {
        // Arrange / Act
        Animal animal = CreateAnimal(Role.Prey);

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
        Action act = () => CreateAnimal(Role.Predator, speed: invalidSpeed);

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
        Action act = () => CreateAnimal(Role.Predator, health: invalidHealth);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenVisionRangeIsNegative_ShouldThrowArgumentException()
    {
        // Arrange / Act
        Action act = () => CreateAnimal(Role.Prey, visionRange: -1);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenSpeciesIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange / Act
        Action act = () => new Animal(null!, default, StubBehavior.StandStill());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WhenBehaviorIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange / Act
        Action act = () => new Animal(TestSpecies.Prey(), default, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void LoseHealth_ShouldReduceHealthByAmount()
    {
        // Arrange
        double moveCost = 0.5;
        double expectedHealth = ValidHealth - moveCost;

        Animal animal = CreateAnimal(Role.Prey);

        // Act
        animal.LoseHealth(moveCost);

        // Assert
        animal.Health.Should().Be(expectedHealth);
        animal.IsAlive.Should().BeTrue();
    }

    [Fact]
    public void LoseHealth_WhenAmountIsMoreThanHealth_ShouldStopAtZeroAndDie()
    {
        // Arrange
        double expectedHealth = 0;

        Animal animal = CreateAnimal(Role.Prey);

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

        Animal predator = CreateAnimal(Role.Predator);
        predator.LoseHealth(damage);

        // Act
        predator.Heal(healing);

        // Assert
        predator.Health.Should().Be(expectedHealth);
    }

    [Fact]
    public void Heal_WhenAmountWouldExceedMaxHealth_ShouldStopAtMaxHealth()
    {
        // Arrange
        double damage = 2;
        double healing = 10;

        Animal predator = CreateAnimal(Role.Predator);
        predator.LoseHealth(damage);

        // Act
        predator.Heal(healing);

        // Assert
        predator.Health.Should().Be(predator.MaxHealth);
    }

    [Fact]
    public void LoseHealth_WhenAmountIsNegative_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        Animal animal = CreateAnimal(Role.Prey);

        // Act
        Action act = () => animal.LoseHealth(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Heal_WhenAmountIsNegative_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        Animal animal = CreateAnimal(Role.Predator);

        // Act
        Action act = () => animal.Heal(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CanEat_WhenOtherIsInPreySet_ShouldReturnTrue()
    {
        // Arrange
        Animal predator = CreateAnimal(Role.Predator);
        Animal prey = CreateAnimal(Role.Prey);

        // Act
        bool canEat = predator.CanEat(prey);

        // Assert
        canEat.Should().BeTrue();
    }

    [Fact]
    public void CanEat_WhenOtherIsNotInPreySet_ShouldReturnFalse()
    {
        // Arrange
        Animal prey = CreateAnimal(Role.Prey);
        Animal predator = CreateAnimal(Role.Predator);

        // Act
        bool canEat = prey.CanEat(predator);

        // Assert
        canEat.Should().BeFalse();
    }

    [Theory]
    [InlineData(Role.Prey)]
    [InlineData(Role.Predator)]
    public void CanEat_WhenOtherIsSameSpecies_ShouldReturnFalse(Role role)
    {
        // Arrange
        Animal first = CreateAnimal(role);
        Animal second = CreateAnimal(role);

        // Act
        bool canEat = first.CanEat(second);

        // Assert
        canEat.Should().BeFalse();
    }

    [Fact]
    public void ToView_ShouldCopyCurrentValues()
    {
        // Arrange
        Animal animal = CreateAnimal(Role.Prey);

        // Act
        AnimalView view = animal.ToView();

        // Assert
        view.Id.Should().Be(animal.Id);
        view.Species.Should().Be(animal.SpeciesId);
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

        Animal animal = CreateAnimal(Role.Prey, startPosition);
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

    // Initialize an animal that plays the given role; each test only passes the values it cares about
    private static Animal CreateAnimal(
        Role role,
        Position position = default,
        double speed = ValidSpeed,
        int visionRange = ValidVisionRange,
        double health = ValidHealth)
    {
        SpeciesDescriptor species = TestSpecies.For(role, speed, visionRange, health);

        return new Animal(species, position, StubBehavior.StandStill());
    }
}
