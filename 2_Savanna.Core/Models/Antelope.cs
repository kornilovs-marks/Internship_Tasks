using Savanna.Core.Behaviors;

namespace Savanna.Core.Models;

public class Antelope(Position position, double speed, int visionRange, double health, AntelopeBehavior behavior)
    : Animal(AnimalKind.Antelope, position, speed, visionRange, health, behavior);