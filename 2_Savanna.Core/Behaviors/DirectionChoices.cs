

using Savanna.Core.Interfaces;
using Savanna.Core.Models;

namespace Savanna.Core.Behaviors;

public static class DirectionChoices
{
    private static readonly Position[] Directions =
    [
        new(0, -1), new(1, 0), new(0, 1), new(-1, 0)
    ];

    // Method to determine a new position for an animal to wander to, based on its current position and the world view
    public static Position Wander(Position origin, IWorldView world, Random random)
    {
        var choices = Directions.Where(direction => world.Contains(origin + direction)).ToArray();
        
        return choices.Length == 0 ? default : choices[random.Next(choices.Length)];
    }

    // Method to determine a new position for an animal to move toward or away from a target position, based on its current position and the world view
    public static Position TowardOrAway(Position origin, Position target, IWorldView world, Random random, bool away)
    {
        var choices = Directions.Where(direction => world.Contains(origin + direction)).ToArray();
        if(choices.Length == 0)
            return default;

        var bestDistance = away ? int.MinValue : int.MaxValue;
        var best = new List<Position>();
        
        foreach(var direction in choices)
        {
            var distance = (origin + direction).DistanceTo(target);
            if((away && distance > bestDistance) ||(!away && distance < bestDistance))
            {
                bestDistance = distance;
                best.Clear();
            }

            if(distance == bestDistance)
                best.Add(direction);
        }

        return best[random.Next(best.Count)];
    }
}
