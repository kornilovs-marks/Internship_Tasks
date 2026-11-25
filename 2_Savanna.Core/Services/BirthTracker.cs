using Savanna.Core.Models;

namespace Savanna.Core.Services;

public class BirthTracker
{
    private readonly int _range;
    private readonly int _roundsRequired;
    private Dictionary<AnimalPair, int> _roundsTogether = [];

    public BirthTracker(int range, int roundsRequired)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(range);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roundsRequired);

        _range = range;
        _roundsRequired = roundsRequired;
    }

    // Records one finished round and returns the pairs that have now been together long enough to give birth
    public IReadOnlyList<BirthPair> RecordRound(IReadOnlyList<Animal> animals)
    {
        var stillTogether = new Dictionary<AnimalPair, int>();
        var readyToGiveBirth = new List<BirthPair>();
        var parentsThisRound = new HashSet<Guid>();

        for (var i = 0; i < animals.Count; i++)
        {
            for (var j = i + 1; j < animals.Count; j++)
            {
                var first = animals[i];
                var second = animals[j];

                if (first.Kind != second.Kind || first.Position.DistanceTo(second.Position) > _range)
                    continue;

                var pair = AnimalPair.Of(first.Id, second.Id);
                var rounds = _roundsTogether.GetValueOrDefault(pair) + 1;

                if (rounds < _roundsRequired)
                {
                    stillTogether[pair] = rounds;
                    continue;
                }

                if (parentsThisRound.Contains(first.Id) || parentsThisRound.Contains(second.Id))
                {
                    stillTogether[pair] = rounds;
                    continue;
                }

                parentsThisRound.Add(first.Id);
                parentsThisRound.Add(second.Id);
                readyToGiveBirth.Add(new BirthPair(first, second));
            }
        }

        _roundsTogether = stillTogether;
        return readyToGiveBirth;
    }

    // The same two animals always produce the same key, whatever order they are given in
    private readonly record struct AnimalPair(Guid First, Guid Second)
    {
        public static AnimalPair Of(Guid a, Guid b) => a.CompareTo(b) < 0 ? new(a, b) : new(b, a);
    }
}
