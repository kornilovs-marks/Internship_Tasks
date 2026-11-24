namespace Savanna.Core.Models;

public record struct AnimalView(
    Guid Id,
    AnimalKind Kind,
    Position Position,
    double Speed,
    int VisionRange,
    double Health
    );
