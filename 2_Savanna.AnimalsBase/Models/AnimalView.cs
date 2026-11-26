namespace Savanna.AnimalsBase.Models;

public record struct AnimalView(
    Guid Id,
    SpeciesId Species,
    Position Position,
    double Speed,
    int VisionRange,
    double Health
    );
