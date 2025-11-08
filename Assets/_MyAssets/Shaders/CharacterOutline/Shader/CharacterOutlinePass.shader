Shader "_MyShader/CharacterOutline/CharacterOutlinePass"
{
    Properties
    {
        [HideInInspector] _GlobalTime("Global Time", Float) = 0
    }

    SubShader
    {
        Tags{ "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Cull Front
            ZTest LEqual
            ZWrite On

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/_MyAssets/Shaders/CharacterOutline/Common/ColorGetter.hlsl"

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
                return GetNowColor();
            }

            ENDCG
        }
    }
}
