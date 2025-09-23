namespace MyScripts.Common;

internal static class BorderLayer
{
    internal static class WalkSound
    {
        private static readonly byte Grass = 10;
        private static readonly byte Sand = 11;
        private static readonly byte Rock = 12;
        private static readonly byte Water = 13;

        internal static byte Get(SWalkSound.Surface surface) => surface switch
        {
            SWalkSound.Surface.Grass => Grass,
            SWalkSound.Surface.Sand => Sand,
            SWalkSound.Surface.Rock => Rock,
            SWalkSound.Surface.Water => Water,
            _ => throw new ArgumentOutOfRangeException(nameof(surface), surface, null)
        };
    }
}
