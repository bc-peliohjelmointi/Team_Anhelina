Shader "Custom/Tvnoisesnow"
{
    Properties
    {
        _Speed ("Speed", Float) = 15        // how fast noise scrolls down
        _Scale ("Noise Scale", Float) = 120 // grain size, higher = smaller grain
        _Intensity ("Intensity", Float) = 2 // overall brightness of noise
        _LineDensity ("Line Density", Float) = 300 // horizontal scan lines frequency
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Speed;
            float _Scale;
            float _Intensity;
            float _LineDensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // classic hash for white noise
            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898,78.233))) * 43758.5453);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // scroll uv over time to make noise move
                uv.y += _Time.y * _Speed;

                float noise = random(floor(uv * _Scale)); // pixelated random noise

                // horizontal scan line effect - bright thin lines
                float lines = frac(uv.y * _LineDensity);
                lines = step(0.95, lines); // only the top 5% of each line period

                float finalValue = noise + lines;
                finalValue *= _Intensity;

                return fixed4(finalValue, finalValue, finalValue, 1); // grayscale output
            }
            ENDCG
        }
    }
}