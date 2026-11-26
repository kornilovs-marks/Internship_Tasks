namespace Savanna.AnimalsBase.Models;

// Everything the engine and the UI need to know about a species; an empty Prey set means a herbivore
public record SpeciesDescriptor(
    SpeciesId Id,
    string DisplayName,
    string Symbol,
    char Key,
    double Speed,
    int VisionRange,
    double Health,
    IReadOnlySet<SpeciesId> Prey);
