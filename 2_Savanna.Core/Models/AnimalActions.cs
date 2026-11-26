namespace Savanna.Core.Models;

// Represents the actions an animal can take in a single turn
public readonly record struct AnimalActions(Position direction, Guid? targetId = null);
