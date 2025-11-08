using System;

namespace foriver4725.ScriptsValidator
{
    internal enum ESrcRoot : byte
    {
        Assets = 0,

        Assets_MyAssets,
        Assets_MyAssets_Scripts,
        Assets_MyAssets_Shaders,
    }

    [Flags]
    internal enum ESrcExtensions : uint
    {
        None = 0,

        Cs = 1 << 0,

        Shader = 1 << 1,
        Hlsl = 1 << 2,

        Txt = 1 << 3,
        Md = 1 << 4,

        Rsp = 1 << 5, // csc
    }

    internal enum EDstEncoding : byte
    {
        DontSpecify = 0,

        UTF8WithBOM,
        UTF8WithoutBOM,
        ShiftJIS,
        ASCII,
    }

    internal enum EDstEndline : byte
    {
        DontSpecify = 0,

        LF,
        CRLF,
    }
}
