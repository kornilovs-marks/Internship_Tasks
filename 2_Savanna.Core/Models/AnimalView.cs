using System;
using System.Collections.Generic;
using System.Text;

namespace _2_Savanna.Core.Models
{
    public record struct AnimalView(
        Guid Id,
        AnimalKind Kind,
        Position Position,
        double Speed,
        int VisionRange
        );
}
