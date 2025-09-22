Shader "_MyShader/CharacterOutline/CharacterBodyMaskingPass"
{
    Properties
    {
        _StencilRef ("Stencil Ref", Int) = 10
    }

    SubShader
    {
        Tags{ "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            ColorMask 0
            ZWrite Off

            Stencil
            {
                Ref [_StencilRef]
                Comp Always
                Pass Replace
            }
        }
    }
}
