Shader "_MyShader/CharacterOutline/CharacterBodyPass"
{
    Properties
    {
        _StencilRef ("Stencil Ref", Int) = 10
    }

    SubShader
    {
        Tags{ "RenderType"="Opaque" "Queue"="Geometry+1" }

        Pass
        {
            ZTest Greater
            ZWrite Off

            Stencil
            {
                Ref [_StencilRef]
                Comp Equal
            }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 pos : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                return o;
            }

            #include "Assets/_MyAssets/Shaders/CharacterOutline/Common/FragmentShader.hlsl"

            ENDCG
        }
    }
}
