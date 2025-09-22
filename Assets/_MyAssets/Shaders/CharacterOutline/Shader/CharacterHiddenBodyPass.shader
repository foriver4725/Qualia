Shader "_MyShader/CharacterOutline/CharacterHiddenBodyPass"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
    #include "Assets/_MyAssets/Shaders/CharacterOutline/Common/ColorGetter.hlsl"

    float4 FullScreenPass(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);

        // カスタム深度値を取得
        float customDepth = LoadCustomDepth(varyings.positionCS.xy);

        // カメラの深度値を取得
        float depth = LoadCameraDepth(varyings.positionCS.xy);

        // 遮蔽されていない部分
        if (customDepth == depth || customDepth == 0)
        {
            // カメラカラーバッファを読み込む
            return float4(CustomPassLoadCameraColor(varyings.positionCS.xy, 0), 1);
        }
        // 遮蔽されている部分
        else
        {
            // 色を描画
            return GetNowColor();
        }
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Custom Pass 0"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
