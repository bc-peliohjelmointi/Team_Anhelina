Shader "Custom/PSGlitch"
{
    Properties
    {
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.1
        _ScanlineSpeed ("Scanline Speed", Float) = 5.0
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

            float _GlitchIntensity;
            float _ScanlineSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float random(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                float scanline = sin(uv.y * 100.0 + _Time.y * _ScanlineSpeed) * 0.1 + 0.9;
                
                float glitch = step(0.99, random(float2(_Time.y, uv.y)));
                uv.x += glitch * (random(float2(_Time.y, uv.y)) - 0.5) * _GlitchIntensity;
                
                float noise = random(uv * _Time.y * 0.1) * 0.1;
                
                float brightness = scanline + noise;
                brightness = lerp(0.2, 1.0, brightness);
                
                fixed4 col = float4(0, brightness, 0, 1);
                
                if (glitch > 0.5)
                {
                    col = float4(0, 1, 0, 1);
                }
                
                return col;
            }
            ENDCG
        }
    }
}