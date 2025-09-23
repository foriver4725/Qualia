#pragma once

#include "Assets/_MyAssets/Shaders/CharacterOutline/Common/RGB2HSV.hlsl"
#include "Assets/_MyAssets/Shaders/CharacterOutline/Common/Parameter.hlsl"

half4 GetNowColor(half elapsedTime)
{
    half3 hsv = RGB2HSV(_BeginColor);
    hsv.x = frac(hsv.x + _ColorChangeSpeed * elapsedTime); // 経過時間でHueを進める(負値なら逆回転)
    half3 rgb = HSV2RGB(hsv);
    return half4(rgb, 1.0);
}
