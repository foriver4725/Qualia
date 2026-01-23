Shader "Hidden/PanoramaCapturer_CubemapToEquirectangular"
{
    Properties
    {
        _Cube ("Cubemap", CUBE) = "" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            samplerCUBE _Cube;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.uv = v.uv;
                return o;
            }

            // Equirectangular UV から方向ベクトルを計算
            // 変換した方向ベクトルを使って、キューブマップから色をサンプリングする
            // uv.x : [0,1] → φ : [-π, π]
            // uv.y : [0,1] → θ : [0, π]
            float3 EquirectangularUVToDir(float2 uv)
            {
                // UVから角度に変換
                // uv : [0,1]
                float phi   = (uv.x * 2.0 - 1.0) * UNITY_PI;      // -π ～ π
                float theta = (1.0 - uv.y) * UNITY_PI;           // 0 ～ π

                // 角度から方向ベクトルに変換
                float3 dir;
                dir.x = sin(theta) * sin(phi);
                dir.y = cos(theta);
                dir.z = sin(theta) * cos(phi);

                return dir;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = EquirectangularUVToDir(i.uv);
                return texCUBE(_Cube, dir);
            }

            ENDHLSL
        }
    }
}
