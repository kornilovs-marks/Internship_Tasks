using FluentAssertions;
using Savanna.AnimalsBase.Factories;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Models;
using Savanna.Core.Services;
using xUnit.Tests.Savanna.TestDoubles;

namespace xUnit.Tests.Savanna;

[Trait("Category", "World")]
public class WorldSnapshotTests
{
    private const int Width = 10;
    private const int Height = 10;

    [Fact]
    public void FindNearby_WhenAnimalsAreInAndOutOfRange_ShouldReturnOnlyThoseInRange()
    {
        // Arrange
        int range = 3;
        int expectedCount = 1;

        var center = new Position(5, 5);

        AnimalView nearAnimal = CreateView(new Position(6, 6));   // distance 2
        AnimalView farAnimal = CreateView(new Position(9, 9));    // distance 8

        WorldSnapshot snapshot = CreateSnapshot(nearAnimal, farAnimal);

        // Act
        var nearby = snapshot.FindNearby(center, range).ToList();

        // Assert
        nearby.Should().HaveCount(expectedCount);
        nearby[0].Id.Should().Be(nearAnimal.Id);
    }

    [Fact]
    public void FindNearby_WhenAnimalIsExactlyAtRange_ShouldIncludeIt()
    {
        // Arrange
        int range = 3;
        int expectedCount = 1;

        var center = new Position(5, 5);

        AnimalView edgeAnimal = CreateView(new Position(7, 6));          // distance 3, exactly at range
        AnimalView justOutsideAnimal = CreateView(new Position(8, 6));   // distance 4

        WorldSnapshot snapshot = CreateSnapshot(edgeAnimal, justOutsideAnimal);

        // Act
        var nearby = snapshot.FindNearby(center, range).ToList();

        // Assert
        nearby.Should().HaveCount(expectedCount);
        nearby[0].Id.Should().Be(edgeAnimal.Id);
    }

    [Theory]
    [InlineData(0, 0, true)]                       // top-left corner
    [InlineData(Width - 1, Height - 1, true)]      // bottom-right corner
    [InlineData(5, 5, true)]                       // middle
    [InlineData(-1, 0, false)]                     // left of field
    [InlineData(Width, 0, false)]                  // right of field
    [InlineData(0, -1, false)]                     // above field
    [InlineData(0, Height, false)]                 // below field
    public void Contains_WhenPositionIsInsideOrOutside_ShouldReturnExpected(int column, int row, bool expected)
    {
        // Arrange
        WorldSnapshot snapshot = CreateSnapshot();
        var position = new Position(column, row);

        // Act
        bool result = snapshot.Contains(position);

        // Assert
        result.Should().Be(expected);
    }

    // Initialize a snapshot of the test field with the given animals; it knows the test species
    private static WorldSnapshot CreateSnapshot(params AnimalView[] animals)
    {
        AnimalFactoryBase[] factories =
        [
            new StubFactory(TestSpecies.Prey()),
            new StubFactory(TestSpecies.Predator())
        ];

        return new WorldSnapshot(Width, Height, animals, new SpeciesRegistry(factories));
    }

    // Initialize a prey view at the given position with a unique id
    private static AnimalView CreateView(Position position)
    {
        return new AnimalView(Guid.NewGuid(), TestSpecies.PreyId, position, 4, 7, 10);
    }
}
