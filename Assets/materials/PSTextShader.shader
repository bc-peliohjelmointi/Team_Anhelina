Shader "Custom/PSTextShader"
{
    Properties
    {
        _MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Text Color", Color) = (0, 1, 0, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 2)) = 1.5
        _ScanlineSpeed ("Scanline Speed", Float) = 2.0
        _Flicker ("Flicker", Range(0, 0.1)) = 0.02
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _GlowIntensity;
            float _ScanlineSpeed;
            float _Flicker;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float scanline = sin(i.uv.y * 100.0 + _Time.y * _ScanlineSpeed) * 0.05 + 0.95;
                
                float flicker = 1.0 - _Flicker + sin(_Time.y * 10.0) * _Flicker;
                
                col.rgb *= _Color.rgb * _GlowIntensity * scanline * flicker;
                col.a *= _Color.a * i.color.a;
                
                return col;
            }
            ENDCG
        }
    }
}