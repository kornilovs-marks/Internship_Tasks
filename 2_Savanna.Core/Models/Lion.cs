using Savanna.Core.Behaviors;

namespace Savanna.Core.Models;

public class Lion(Position position, double speed, int visionRange, LionBehavior behavior)
    : Animal(AnimalKind.Lion, position, speed, visionRange, behavior);
