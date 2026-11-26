using Savanna.AnimalsBase.Factories;
using Savanna.AnimalsBase.Models;

namespace Savanna.Core.Services;

public class SpeciesRegistry
{
    private readonly Dictionary<SpeciesId, SpeciesDescriptor> _byId = [];

    public IReadOnlyList<SpeciesDescriptor> All { get; }

    public SpeciesRegistry(IEnumerable<AnimalFactoryBase> factories)
    {
        var all = new List<SpeciesDescriptor>();

        foreach (var species in factories.Select(static factory => factory.Species))
        {
            if (!_byId.TryAdd(species.Id, species))
                throw new InvalidOperationException($"Species {species.Id} is registered more than once.");

            all.Add(species);
        }

        All = all.AsReadOnly();
    }

    public bool TryGet(SpeciesId id, out SpeciesDescriptor species) => _byId.TryGetValue(id, out species!);

    public bool CanEat(SpeciesId eater, SpeciesId food) =>
        _byId.TryGetValue(eater, out var species) && species.Prey.Contains(food);
}
