half3 RGB2HSV(half3 rgb)
{
    half r = rgb.r, g = rgb.g, b = rgb.b;

    half4 K = half4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
    half4 p = (g < b) ? half4(b, g, K.w, K.z) : half4(g, b, K.x, K.y);
    half4 q = (r < p.x) ? half4(p.x, p.y, p.w, r) : half4(r, g, p.y, p.z);

    half d = q.x - min(q.w, q.y);
    half e = 1e-6;

    half3 hsv;
    hsv.x = abs(q.z + (q.w - q.y) / (6.0 * d + e));
    hsv.y = d / (q.x + e);
    hsv.z = q.x;
    return hsv;
}

half3 HSV2RGB(half3 hsv)
{
    half3 rgb = clamp(abs(frac(hsv.xxx + half3(0.0, 2.0/6.0, 4.0/6.0)) * 6.0 - 3.0) - 1.0, 0.0, 1.0);
    return hsv.z * lerp(half3(1.0,1.0,1.0), rgb, hsv.y);
}