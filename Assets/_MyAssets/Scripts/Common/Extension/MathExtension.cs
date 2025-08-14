namespace MyScripts.Common.Extension;

internal static class MathExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float Remap(this int x, int a, int b, float c, float d) => (x - a) * (d - c) / (b - a) + c;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float Remap(this float x, float a, float b, float c, float d) => (x - a) * (d - c) / (b - a) + c;
}
