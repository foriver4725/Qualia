Shader "MyShader/CharacterOutline"
{
    Properties
    {
        _BeginColor ("Begin Color", Color) = (0,0,0,1)
        _Width ("Width", Range(0, 0.1)) = 0.01
        _ColorChangeSpeed ("Color Change Speed", Range(-5, 5)) = 0.2
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
            #include "./RGB2HSV.hlsl"

            half3 _BeginColor;
            half _Width;
            half _ColorChangeSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
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
                half3 hsv = RGB2HSV(_BeginColor);
                hsv.x = frac(hsv.x + _ColorChangeSpeed * _Time.y); // 経過時間でHueを進める(負値なら逆回転)
                half3 rgb = HSV2RGB(hsv);
                return half4(rgb, 1.0);
            }

            ENDCG
        }
    }
}