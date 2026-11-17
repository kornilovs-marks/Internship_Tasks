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

        options.AntelopeSpeed.Should().Be(GameDefaults.AntelopeSpeed);
        options.AntelopeVision.Should().Be(GameDefaults.AntelopeVisionRange);

        options.LionSpeed.Should().Be(GameDefaults.LionSpeed);
        options.LionVision.Should().Be(GameDefaults.LionVisionRange);
    }

    [Fact]
    public void Constructor_WhenValuesProvided_ShouldOverrideDefaults()
    {
        // Arrange
        int customWidth = 10;
        int customHeight = 5;
        double customLionSpeed = 2.5;

        // Act
        var options = new GameOptions
        {
            FieldWidth = customWidth,
            FieldHeight = customHeight,
            LionSpeed = customLionSpeed
        };

        // Assert
        options.FieldWidth.Should().Be(customWidth);
        options.FieldHeight.Should().Be(customHeight);
        options.LionSpeed.Should().Be(customLionSpeed);

        options.AntelopeSpeed.Should().Be(GameDefaults.AntelopeSpeed);
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
        int smallestSize = 1;
        int smallestTick = 1;
        int smallestVision = 1;
        double smallestSpeed = double.Epsilon;

        var options = new GameOptions
        {
            FieldWidth = smallestSize,
            FieldHeight = smallestSize,
            TickMilliseconds = smallestTick,
            AntelopeSpeed = smallestSpeed,
            AntelopeVision = smallestVision,
            LionSpeed = smallestSpeed,
            LionVision = smallestVision
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
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
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
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
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
}