using Savanna.Core.Configuration;

namespace Savanna.Core.Models;

public class GameWorld
{
    private List<Animal> _animals = [];

    public int Width { get; }
    public int Height { get; }
    internal IReadOnlyList<Animal> Animals => _animals;

    // Constructor that initializes the game world with specified options and validates them
    public GameWorld(GameOptions options)
    {
        options.Validate();
        Width = options.FieldWidth;
        Height = options.FieldHeight;
    }

    // Checks if a given position is within the bounds of the grid
    public bool Contains(Position position) => position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;

    // Creates a read-only copy of the current worl state
    public WorldSnapshot Snapshot() => new(Width, Height, _animals.Select(animal => animal.ToView()));

    // Adds an animal to the game world
    internal void Add(Animal animal)
    {
        if(!Contains(animal.Position))
            throw new ArgumentOutOfRangeException(nameof(animal.Position), "Animal position is out of bounds.");

        _animals.Add(animal);
    }

    // Moves an animal to a new position within the game world
    internal void Move(Animal animal, Position destination)
    {
        if(!Contains(destination))
            throw new ArgumentOutOfRangeException(nameof(destination), "Destination position is out of bounds.");

        animal.Position = destination;
    }

    internal void Remove(Animal animal) => _animals.Remove(animal);
}
