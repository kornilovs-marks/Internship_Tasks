namespace _2_Savanna.Core.Models
{
    public record struct AnimalActions(Position direction, Guid? targetId = null)
    {
        public static AnimalActions Stay => new(new Position(0, 0));
    }
}
