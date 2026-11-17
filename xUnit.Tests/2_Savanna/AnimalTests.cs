using FluentAssertions;
using Savanna.Core.Behaviors;
using Savanna.Core.Models;

namespace xUnit.Tests.Savanna;

[Trait("Category", "Models")]
public class AnimalTests
{
    private const double ValidSpeed = 5;
    private const int ValidVisionRange = 9;

    [Fact]
    public void Constructor_WhenLionCreated_ShouldSetKindAndProperties()
    {
        // Arrange
        double speed = 5;
        int visionRange = 9;

        var position = new Position(3, 4);
        var behavior = new LionBehavior(new Random());

        // Act
        var lion = new Lion(position, speed, visionRange, behavior);

        // Assert
        lion.Kind.Should().Be(AnimalKind.Lion);
        lion.Position.Should().Be(position);
        lion.Speed.Should().Be(speed);
        lion.VisionRange.Should().Be(visionRange);
        lion.Behavior.Should().BeSameAs(behavior);
    }

    [Fact]
    public void Constructor_WhenAntelopeCreated_ShouldSetKindAndProperties()
    {
        // Arrange
        double speed = 4;
        int visionRange = 7;

        var position = new Position(1, 2);
        var behavior = new AntelopeBehavior(new Random());

        // Act
        var antelope = new Antelope(position, speed, visionRange, behavior);

        // Assert
        antelope.Kind.Should().Be(AnimalKind.Antelope);
        antelope.Position.Should().Be(position);
        antelope.Speed.Should().Be(speed);
        antelope.VisionRange.Should().Be(visionRange);
        antelope.Behavior.Should().BeSameAs(behavior);
    }

    [Fact]
    public void Constructor_WhenCreatedTwice_ShouldAssignUniqueIds()
    {
        // Arrange / Act
        Animal firstLion = CreateLion();
        Animal secondLion = CreateLion();

        // Assert
        firstLion.Id.Should().NotBe(secondLion.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_WhenSpeedIsInvalid_ShouldThrowArgumentException(double invalidSpeed)
    {
        // Arrange
        var position = new Position(0, 0);
        var behavior = new LionBehavior(new Random());

        // Act
        Action act = () => new Lion(position, invalidSpeed, ValidVisionRange, behavior);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenVisionRangeIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        int invalidVisionRange = -1;

        var position = new Position(0, 0);
        var behavior = new AntelopeBehavior(new Random());

        // Act
        Action act = () => new Antelope(position, ValidSpeed, invalidVisionRange, behavior);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenVisionRangeIsZero_ShouldNotThrow()
    {
        // Arrange
        int zeroVisionRange = 0;

        var position = new Position(0, 0);
        var behavior = new AntelopeBehavior(new Random());

        // Act
        Action act = () => new Antelope(position, ValidSpeed, zeroVisionRange, behavior);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ToView_ShouldCopyCurrentValues()
    {
        // Arrange
        Animal lion = CreateLion();

        // Act
        AnimalView view = lion.ToView();

        // Assert
        view.Id.Should().Be(lion.Id);
        view.Kind.Should().Be(lion.Kind);
    }

    // Initialize a valid lion for tests that do not care about its values
    private static Lion CreateLion()
    {
        var position = new Position(0, 0);

        return new Lion(position, ValidSpeed, ValidVisionRange, new LionBehavior(new Random()));
    }
}