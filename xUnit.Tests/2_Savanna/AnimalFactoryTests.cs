using FluentAssertions;
using Savanna.AnimalsBase.Factories;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Models;
using Savanna.Core.Services;
using xUnit.Tests.Savanna.TestDoubles;

namespace xUnit.Tests.Savanna;

[Trait("Category", "Factories")]
public class AnimalFactoryTests
{
    [Theory]
    [InlineData(Role.Prey)]
    [InlineData(Role.Predator)]
    public void Create_WhenSpeciesIsRegistered_ShouldBuildAnimalFromThatSpecies(Role role)
    {
        // Arrange
        double speed = 6;
        int vision = 11;

        var position = new Position(2, 3);
        SpeciesDescriptor species = TestSpecies.For(role, speed, vision);

        AnimalFactory factory = CreateFactory(new StubFactory(species), new StubFactory(OtherRole(role)));

        // Act
        Animal animal = factory.Create(species.Id, position);

        // Assert
        animal.Species.Should().BeSameAs(species);
        animal.SpeciesId.Should().Be(TestSpecies.IdFor(role));
        animal.Position.Should().Be(position);
        animal.Speed.Should().Be(speed);
        animal.VisionRange.Should().Be(vision);
    }

    [Fact]
    public void Create_ShouldUseBehaviorFromSpeciesFactory()
    {
        // Arrange
        AnimalFactory factory = CreateFactory(new StubFactory(TestSpecies.Prey()));

        // Act
        Animal animal = factory.Create(TestSpecies.PreyId, new Position(0, 0));

        // Assert
        animal.Behavior.Should().BeOfType<StubBehavior>();
    }

    [Fact]
    public void Create_WhenCalledTwice_ShouldGiveEachAnimalItsOwnBehavior()
    {
        // Arrange
        AnimalFactory factory = CreateFactory(new StubFactory(TestSpecies.Prey()));

        // Act
        Animal first = factory.Create(TestSpecies.PreyId, new Position(0, 0));
        Animal second = factory.Create(TestSpecies.PreyId, new Position(1, 0));

        // Assert
        first.Behavior.Should().NotBeSameAs(second.Behavior);
    }

    [Fact]
    public void Create_WhenNoFactoryRegisteredForSpecies_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var position = new Position(0, 0);
        AnimalFactory onlyPrey = CreateFactory(new StubFactory(TestSpecies.Prey()));

        // Act
        Action act = () => onlyPrey.Create(TestSpecies.PredatorId, position);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_WhenTwoFactoriesHaveSameSpecies_ShouldThrowArgumentException()
    {
        // Arrange
        AnimalFactoryBase[] duplicateFactories =
        [
            new StubFactory(TestSpecies.Predator()),
            new StubFactory(TestSpecies.Predator())
        ];

        // Act
        Action act = () => new AnimalFactory(duplicateFactories);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // Initialize the engine's factory with the given species factories
    private static AnimalFactory CreateFactory(params AnimalFactoryBase[] factories)
    {
        return new AnimalFactory(factories);
    }

    // The species for the other role, so every test registers both and the factory has to pick the right one
    private static SpeciesDescriptor OtherRole(Role role) =>
        role == Role.Predator ? TestSpecies.Prey() : TestSpecies.Predator();
}