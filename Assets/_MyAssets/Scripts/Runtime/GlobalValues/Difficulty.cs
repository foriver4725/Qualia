namespace MyScripts.Runtime;

internal static partial class GlobalValues
{
    internal static Difficulty Difficulty { get; set; } = Difficulty.Normal;

    internal static int GetSOSSignPlaceAmount() => Difficulty switch
    {
        Difficulty.Easy => 3,
        Difficulty.Normal => 5,
        Difficulty.Hard => 10,
        _ => throw new ArgumentOutOfRangeException(nameof(Difficulty), Difficulty, null)
    };
}
