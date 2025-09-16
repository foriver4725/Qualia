Shader "Unlit/Color_CustomQueue"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)

        // ▼Render Queueをインスペクターから変更可能にする
        // 代表的なキューを列挙しておく

    }
    SubShader
    {
        // ここでQueueタグにプロパティを埋め込み
        Tags { "RenderType"="Opaque" "Queue"="Transparent+1" }
        LOD 100
        ZWrite Off
        ZTest Always

        Pass
        {
            // そのまま単色出力するUnlitパス
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
