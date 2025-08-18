Shader "MyShader/CharacterOutline"
{
    Properties
    {
        _BeginColor ("Begin Color", Color) = (0, 0, 0, 1)
        _Width ("Width", Range(0, 0.1)) = 0.01
        _ColorChangeSpeed ("Color Change Speed", Range(-5, 5)) = 0.2
    }

    SubShader
    {
        Tags{ "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "FullPass"

            Cull Front
            ZTest LEqual
            ZWrite On

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
                float4 pos : POSITION;
                float3 norm : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;

                // 頂点座標
                // MVP
                o.pos = UnityObjectToClipPos(v.pos);

                // 法線
                // MV (本来はここまで)
                float3 normVS = mul((float3x3)UNITY_MATRIX_IT_MV, v.norm);
                // 無理矢理 PS に変換
                // ↓↓ の計算と同じ, より最適化されている
                /*
                * // 奥行きは無視、正規化するのでw除算すらも必要ない
                * float2 normPS = normalize(mul(UNITY_MATRIX_P, float4(normVS, 1.0)).xy);
                */
                // 参考 : https://gist.github.com/hecomi/9580605
                float2 normPS = normalize(TransformViewToProjection(normVS.xy));

                // PS での法線の方向に、頂点座標をオフセット
                // w乗算することで、頂点座標の遠近を打ち消す、的な感じ
                o.pos.xy += normPS * (_Width * o.pos.w);

                return o;
            }

            half4 frag(v2f _) : SV_Target
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