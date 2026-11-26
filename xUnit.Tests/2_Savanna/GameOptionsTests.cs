using FluentAssertions;
using Savanna.Core.Configuration;
using Savanna.Core.Constants;

namespace xUnit.Tests.Savanna;

[Trait("Category", "Configuration")]
public class GameOptionsTests
{
    [Fact]
    public void Constructor_WhenNoValuesProvided_ShouldUseGameDefaults()
    {
        // Arrange / Act
        var options = new GameOptions();

        // Assert
        options.FieldWidth.Should().Be(GameDefaults.FieldWidth);
        options.FieldHeight.Should().Be(GameDefaults.FieldHeight);
        options.TickMilliseconds.Should().Be(GameDefaults.TickMilliseconds);
        options.RoundMilliseconds.Should().Be(GameDefaults.RoundMilliseconds);

        options.AntelopeSpeed.Should().Be(GameDefaults.AntelopeSpeed);
        options.AntelopeVision.Should().Be(GameDefaults.AntelopeVisionRange);
        options.AntelopeHealth.Should().Be(GameDefaults.AntelopeHealth);

        options.LionSpeed.Should().Be(GameDefaults.LionSpeed);
        options.LionVision.Should().Be(GameDefaults.LionVisionRange);
        options.LionHealth.Should().Be(GameDefaults.LionHealth);

        options.BirthRange.Should().Be(GameDefaults.BirthRange);
        options.BirthRoundsRequired.Should().Be(GameDefaults.BirthRoundsRequired);
        options.HealthGainOnEat.Should().Be(GameDefaults.HealthGainOnEat);
        options.MoveHealthCost.Should().Be(GameDefaults.MoveHealthCost);
    }

    [Fact]
    public void Validate_WhenOptionsAreDefault_ShouldNotThrow()
    {
        // Arrange
        var options = new GameOptions();

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WhenValuesAreSmallestAllowed_ShouldNotThrow()
    {
        // Arrange
        int smallestPositiveInt = 1;
        double smallestPositiveDouble = double.Epsilon;

        var options = new GameOptions
        {
            FieldWidth = smallestPositiveInt,
            FieldHeight = smallestPositiveInt,
            TickMilliseconds = smallestPositiveInt,
            RoundMilliseconds = smallestPositiveInt,
            AntelopeSpeed = smallestPositiveDouble,
            AntelopeVision = smallestPositiveInt,
            AntelopeHealth = smallestPositiveDouble,
            LionSpeed = smallestPositiveDouble,
            LionVision = smallestPositiveInt,
            LionHealth = smallestPositiveDouble,
            BirthRoundsRequired = smallestPositiveInt,
            BirthRange = 0,
            MoveHealthCost = 0,
            HealthGainOnEat = 0
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenFieldWidthIsNotPositive_ShouldThrowArgumentException(int invalidWidth)
    {
        // Arrange
        var options = new GameOptions { FieldWidth = invalidWidth };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenFieldHeightIsNotPositive_ShouldThrowArgumentException(int invalidHeight)
    {
        // Arrange
        var options = new GameOptions { FieldHeight = invalidHeight };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenTickIsNotPositive_ShouldThrowArgumentException(int invalidTick)
    {
        // Arrange
        var options = new GameOptions { TickMilliseconds = invalidTick };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenRoundMillisecondsIsNotPositive_ShouldThrowArgumentException(int invalidRound)
    {
        // Arrange
        var options = new GameOptions { RoundMilliseconds = invalidRound };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Validate_WhenAntelopeSpeedIsInvalid_ShouldThrowArgumentException(double invalidSpeed)
    {
        // Arrange
        var options = new GameOptions { AntelopeSpeed = invalidSpeed };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Validate_WhenLionSpeedIsInvalid_ShouldThrowArgumentException(double invalidSpeed)
    {
        // Arrange
        var options = new GameOptions { LionSpeed = invalidSpeed };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenAntelopeVisionIsNotPositive_ShouldThrowArgumentException(int invalidVision)
    {
        // Arrange
        var options = new GameOptions { AntelopeVision = invalidVision };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenLionVisionIsNotPositive_ShouldThrowArgumentException(int invalidVision)
    {
        // Arrange
        var options = new GameOptions { LionVision = invalidVision };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Validate_WhenAntelopeHealthIsInvalid_ShouldThrowArgumentException(double invalidHealth)
    {
        // Arrange
        var options = new GameOptions { AntelopeHealth = invalidHealth };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Validate_WhenLionHealthIsInvalid_ShouldThrowArgumentException(double invalidHealth)
    {
        // Arrange
        var options = new GameOptions { LionHealth = invalidHealth };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenBirthRoundsRequiredIsNotPositive_ShouldThrowArgumentException(int invalidRounds)
    {
        // Arrange
        var options = new GameOptions { BirthRoundsRequired = invalidRounds };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Validate_WhenBirthRangeIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new GameOptions { BirthRange = -1 };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Validate_WhenMoveHealthCostIsInvalid_ShouldThrowArgumentException(double invalidCost)
    {
        // Arrange
        var options = new GameOptions { MoveHealthCost = invalidCost };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Validate_WhenHealthGainOnEatIsInvalid_ShouldThrowArgumentException(double invalidGain)
    {
        // Arrange
        var options = new GameOptions { HealthGainOnEat = invalidGain };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
