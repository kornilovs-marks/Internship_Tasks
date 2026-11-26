using Savanna.Core.Configuration;
using Savanna.Core.Interfaces;
using Savanna.AnimalsBase.Models;
using Savanna.Core.Models;

namespace Savanna.Core.Services;

public class AnimalBirth(GameWorld world, IAnimalFactory factory, Random random, GameOptions options)
{
    private readonly GameWorld _world = world;
    private readonly IAnimalFactory _factory = factory;
    private readonly Random _random = random;
    private readonly TimeSpan _roundDuration = TimeSpan.FromMilliseconds(options.RoundMilliseconds);
    private readonly BirthTracker _births = new(options.BirthRange, options.BirthRoundsRequired);
    private TimeSpan _timeSinceRound = TimeSpan.Zero;

    // Counts game time and finishes a round each time a full round has passed
    public void AdvanceRounds(TimeSpan elapsed)
    {
        _timeSinceRound += elapsed;

        while (_timeSinceRound >= _roundDuration)
        {
            _timeSinceRound -= _roundDuration;
            CompleteRound();
        }
    }

    // Birth: every pair that stayed together long enough gets one newborn
    private void CompleteRound()
    {
        var readyPairs = _births.RecordRound(_world.Animals);

        foreach (var (first, second) in readyPairs)
            GiveBirthIfRoomAvailable(first, second);
    }

    // Places the newborn on a free cell next to one of its parents; no free cell means no birth
    private void GiveBirthIfRoomAvailable(Animal first, Animal second)
    {
        var occupied = _world.Animals.Select(static animal => animal.Position).ToHashSet();

        var freeCells = Position.Directions
            .SelectMany(offset => new[] { first.Position + offset, second.Position + offset })
            .Where(cell => _world.Contains(cell) && !occupied.Contains(cell))
            .Distinct()
            .ToList();

        if (freeCells.Count == 0)
            return;

        var position = freeCells[_random.Next(freeCells.Count)];
        _world.Add(_factory.Create(first.SpeciesId, position));
    }
}
