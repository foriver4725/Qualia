Shader "MyShader/CharacterOutline"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)
        _Width ("Width", Range(0,0.1)) = 0.01
    }

    SubShader
    {
        Tags{ "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Cull Front
            ZWrite On
            ZTest LEqual

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            half4 _Color;
            half _Width;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;

                // MVP座標変換
                o.pos = UnityObjectToClipPos(v.vertex);

                // 法線の座標変換 : モデル空間 -> カメラ空間
                float3 norm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));

                // 投影
                float2 offset = TransformViewToProjection(norm.xy);

                // クリップ空間で頂点を動かす
                o.pos.xy += offset * o.pos.w * _Width;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return _Color;
            }

            ENDCG
        }
    }
}