Shader "MyShader/BarrierBorder"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0, 0, 0.5)
        _Div ("Stripe Division Amount (when parallel to the axis)", Range(1, 1000)) = 10
        _SlopeX ("Stripe Slope X (auto normalized to be Y is positive)", Float) = 1.0
        _SlopeY ("Stripe Slope Y (auto normalized to be Y is positive)", Float) = 1.0
        _MoveSpeed ("Stripe Slope Move Speed (clockwise is negative)", Range(-5, 5)) = 1.0

        // スクリプトからのみアクセスするプロパティ
        // 初期値は、エディタ上で設定しておく
        [MaterialToggle] _Enabled ("Enabled", Float) = 1 // 0: Off, 1: On
        // うっすら消す、みたいな機能で使う
        _TransparencyCoefficient ("Transparency Coefficient", Range(0, 1)) = 1.0 // 0: 完全透明, 1: 完全不透明
    }

    SubShader
    {
        Tags{ "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Cull Back
            ZTest LEqual
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            half4 _Color;
            half _Div;
            half _SlopeX;
            half _SlopeY;
            half _MoveSpeed;

            // スクリプトからのみアクセスするプロパティ
            half _Enabled;
            half _TransparencyCoefficient;

            struct appdata
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // オフの時は、完全な透明
                if (_Enabled < 0.5) return 0;

                // 使いやすいように初期計算
                half2 slope = normalize(half2(_SlopeX, _SlopeY));
                slope *= lerp(step(0, slope.y), 1, -1); // 傾きのy座標は正であってほしいので、もしそうでないなら象限を原点対象に反転する
                half theta = atan2(slope.y, slope.x); // 傾きのなす角

                // UV座標を -theta 回転し、縦方向にストライプがつくようにすれば良い
                half2 uvRotated = mul(half2x2(
                    cos(theta), sin(theta),
                    -sin(theta), cos(theta)
                ), i.uv);

                // _Div 分割し、ストライプの箇所ならそのままのアルファ、そうでないならアルファを0にして透明にする
                half aCoef = step(0.5, frac(uvRotated.y * _Div - _MoveSpeed * _Time.y)); // アルファの係数 (0 or 1)
                return half4(_Color.rgb, _Color.a * aCoef * _TransparencyCoefficient);
            }

            ENDCG
        }
    }
}