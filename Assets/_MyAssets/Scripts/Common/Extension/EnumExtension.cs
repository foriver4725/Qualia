using UnityEngine.Assertions;

namespace MyScripts.Common.Extension
{
    internal static class EnumExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TInteger ToInteger<TEnum, TInteger>(this TEnum value)
            where TEnum : struct, Enum
            where TInteger : unmanaged
        {
            Assert.IsTrue(typeof(TInteger) == Enum.GetUnderlyingType(typeof(TEnum)));

            return Unsafe.As<TEnum, TInteger>(ref value);
        }
    }
}
