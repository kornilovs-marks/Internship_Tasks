using FluentAssertions;
using Savanna.Core.Configuration;
using Savanna.Core.Factories;
using Savanna.Core.Models;
using Savanna.Core.Services;

namespace xUnit.Tests.Savanna;

[Trait("Category", "Factories")]
public class AnimalFactoryTests
{
    [Fact]
    public void Create_WhenKindIsLion_ShouldReturnLionWithLionOptions()
    {
        // Arrange
        double lionSpeed = 6;
        int lionVision = 11;

        var position = new Position(2, 3);
        var options = new GameOptions { LionSpeed = lionSpeed, LionVision = lionVision };

        AnimalFactory factory = CreateFactory(options);

        // Act
        Animal animal = factory.Create(AnimalKind.Lion, position);

        // Assert
        animal.Should().BeOfType<Lion>();
        animal.Kind.Should().Be(AnimalKind.Lion);
        animal.Position.Should().Be(position);
        animal.Speed.Should().Be(lionSpeed);
        animal.VisionRange.Should().Be(lionVision);
    }

    [Fact]
    public void Create_WhenKindIsAntelope_ShouldReturnAntelopeWithAntelopeOptions()
    {
        // Arrange
        double antelopeSpeed = 3;
        int antelopeVision = 5;

        var position = new Position(1, 4);
        var options = new GameOptions { AntelopeSpeed = antelopeSpeed, AntelopeVision = antelopeVision };

        AnimalFactory factory = CreateFactory(options);

        // Act
        Animal animal = factory.Create(AnimalKind.Antelope, position);

        // Assert
        animal.Should().BeOfType<Antelope>();
        animal.Kind.Should().Be(AnimalKind.Antelope);
        animal.Position.Should().Be(position);
        animal.Speed.Should().Be(antelopeSpeed);
        animal.VisionRange.Should().Be(antelopeVision);
    }

    [Fact]
    public void Create_WhenNoFactoryRegisteredForKind_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var options = new GameOptions();
        var random = new Random();
        var position = new Position(0, 0);

        var onlyAntelopes = new AnimalFactoryBase[]
        {
            new AntelopeFactory(options, random)
        };

        var factory = new AnimalFactory(onlyAntelopes);

        // Act
        Action act = () => factory.Create(AnimalKind.Lion, position);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_WhenTwoFactoriesHaveSameKind_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new GameOptions();
        var random = new Random();

        var duplicateFactories = new AnimalFactoryBase[]
        {
            new LionFactory(options, random),
            new LionFactory(options, random)
        };

        // Act
        Action act = () => new AnimalFactory(duplicateFactories);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // Initialize the selector with both real factories using the given options
    private static AnimalFactory CreateFactory(GameOptions options)
    {
        var random = new Random();
        var factories = new AnimalFactoryBase[]
        {
            new AntelopeFactory(options, random),
            new LionFactory(options, random)
        };

        return new AnimalFactory(factories);
    }
}