using Savanna.Core.Behaviors;

namespace Savanna.Core.Models;

public class Antelope(Position position, double speed, int visionRange, AntelopeBehavior behavior)
    : Animal(AnimalKind.Antelope, position, speed, visionRange, behavior);