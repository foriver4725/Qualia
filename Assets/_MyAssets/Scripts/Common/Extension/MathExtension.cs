namespace MyScripts.Common.Extension;

internal static class MathExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float Remap(this int x, int a, int b, float c, float d) => (x - a) * (d - c) / (b - a) + c;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float Remap(this float x, float a, float b, float c, float d) => (x - a) * (d - c) / (b - a) + c;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float RemapClamped(this int x, int a, int b, float c, float d)
    {
        if (a < b)
        {
            if (x <= a) return c;
            if (x >= b) return d;
            return x.Remap(a, b, c, d);
        }
        else
        {
            if (x >= a) return c;
            if (x <= b) return d;
            return x.Remap(b, a, d, c);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float RemapClamped(this float x, float a, float b, float c, float d)
    {
        if (a < b)
        {
            if (x <= a) return c;
            if (x >= b) return d;
            return x.Remap(a, b, c, d);
        }
        else
        {
            if (x >= a) return c;
            if (x <= b) return d;
            return x.Remap(b, a, d, c);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float Cross(this Vector2 lhs, Vector2 rhs) => lhs.x * rhs.y - lhs.y * rhs.x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float CrossSqr(this Vector2 lhs, Vector2 rhs)
    {
        float cross = lhs.Cross(rhs);
        return cross * cross;
    }
}
