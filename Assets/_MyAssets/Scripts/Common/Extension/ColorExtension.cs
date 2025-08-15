namespace MyScripts.Common.Extension
{
    /// <summary>
    /// ColorUtility.ToHtmlString() 系統のメソッドに対して、内部処理を ZString に置き換えた拡張メソッド
    /// </summary>
    internal static class Extension
    {
        //
        // Summary:
        //     Returns the color as a hexadecimal string in the format "RRGGBB".
        //
        // Parameters:
        //   color:
        //     The color to be converted.
        //
        // Returns:
        //     Hexadecimal string representing the color.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToHtmlStringRGB(this Color color)
        {
            Color32 color2 = new Color32((byte)Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255), (byte)Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255), (byte)Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255), 1);
            return ZString.Format("{0:X2}{1:X2}{2:X2}", color2.r, color2.g, color2.b);
        }

        //
        // Summary:
        //     Returns the color as a hexadecimal string in the format "RRGGBBAA".
        //
        // Parameters:
        //   color:
        //     The color to be converted.
        //
        // Returns:
        //     Hexadecimal string representing the color.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ToHtmlStringRGBA(this Color color)
        {
            Color32 color2 = new Color32((byte)Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255), (byte)Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255), (byte)Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255), (byte)Mathf.Clamp(Mathf.RoundToInt(color.a * 255f), 0, 255));
            return ZString.Format("{0:X2}{1:X2}{2:X2}{3:X2}", color2.r, color2.g, color2.b, color2.a);
        }
    }
}