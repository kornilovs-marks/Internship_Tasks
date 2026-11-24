using Savanna.Core.Constants;

namespace Savanna.Core.Configuration;

public class GameOptions
{
    public int FieldWidth { get; init; } = GameDefaults.FieldWidth;
    public int FieldHeight { get; init; } = GameDefaults.FieldHeight;
    public int TickMilliseconds { get; init; } = GameDefaults.TickMilliseconds;
    public double AntelopeSpeed { get; init; } = GameDefaults.AntelopeSpeed;
    public int AntelopeVision { get; init; } = GameDefaults.AntelopeVisionRange;
    public double LionSpeed { get; init; } = GameDefaults.LionSpeed;
    public int LionVision { get; init; } = GameDefaults.LionVisionRange;
    public double AntelopeHealth { get; init; } = GameDefaults.AntelopeHealth;
    public double LionHealth { get; init; } = GameDefaults.LionHealth;
    public double MoveHealthCost { get; init; } = GameDefaults.MoveHealthCost;
    public double LionHealthGainOnEat { get; init; } = GameDefaults.LionHealthGainOnEat;
    public int RoundMilliseconds { get; init; } = GameDefaults.RoundMilliseconds;
    public int BirthRoundsRequired { get; init; } = GameDefaults.BirthRoundsRequired;
    public int BirthRange { get; init; } = GameDefaults.BirthRange;

    // Validation method to ensure that the game options are set correctly
    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(FieldWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(FieldHeight);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(TickMilliseconds);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(AntelopeVision);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(LionVision);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(RoundMilliseconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(BirthRoundsRequired);
        ArgumentOutOfRangeException.ThrowIfNegative(BirthRange);

        if (!double.IsFinite(AntelopeSpeed) || AntelopeSpeed <= 0 || !double.IsFinite(LionSpeed) || LionSpeed <= 0)
            throw new ArgumentException(nameof(GameOptions), "Animal speeds must be positive finite numbers");

        if (!double.IsFinite(AntelopeHealth) || AntelopeHealth <= 0 || !double.IsFinite(LionHealth) || LionHealth <= 0)
            throw new ArgumentException("Animal health must be a positive finite number", nameof(AntelopeHealth));

        if (!double.IsFinite(MoveHealthCost) || MoveHealthCost < 0)
            throw new ArgumentException("Move health cost cannot be negative", nameof(MoveHealthCost));

        if (!double.IsFinite(LionHealthGainOnEat) || LionHealthGainOnEat < 0)
            throw new ArgumentException("Health gained from eating cannot be negative", nameof(LionHealthGainOnEat));
    }
}
