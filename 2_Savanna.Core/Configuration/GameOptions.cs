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
    public int LionVision { get; init; } = GameDefaults.AntelopeVisionRange;

    // Validation method to ensure that the game options are set correctly
    public void Validate()
    {
        if (FieldWidth <= 0 || FieldHeight <= 0)
            throw new ArgumentException(nameof(GameOptions), "Field dimensions must be positive");
        
        if (TickMilliseconds <= 0)
            throw new ArgumentException(nameof(GameOptions), "Tick milliseconds must be positive");
        
        if (!double.IsFinite(AntelopeSpeed) || AntelopeSpeed <= 0 || !double.IsFinite(LionSpeed) || LionSpeed <= 0)
            throw new ArgumentException(nameof(GameOptions), "Animal speeds must be positive finite numbers");
        
        if(AntelopeVision <= 0 || LionVision <= 0)
            throw new ArgumentException(nameof(GameOptions), "Animal vision ranges must be positive");
    }
}
