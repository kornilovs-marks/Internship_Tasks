using Savanna.AnimalsBase.Interfaces;
using Savanna.AnimalsBase.Models;

namespace xUnit.Tests.Savanna.TestDoubles;

public class StubBehavior(AnimalActions action) : IAnimalBehavior
{
    public static readonly Position Up = new(0, -1);
    public static readonly Position Right = new(1, 0);
    public static readonly Position Down = new(0, 1);
    public static readonly Position Left = new(-1, 0);

    public static StubBehavior StandStill() => new(default);

    public static StubBehavior Step(Position direction) => new(new AnimalActions(direction));

    public static StubBehavior Target(Guid targetId) => new(new AnimalActions(default, targetId));

    public virtual AnimalActions ChooseAction(AnimalView animal, IWorldView world) => action;
}
