Shader "Custom/RetroTextReadable"
{
    Properties
    {
        _MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Text Color", Color) = (0, 1, 0, 1)
        
        [Header(Glow Effect)]
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1.2
        _InnerGlow ("Inner Glow", Range(0, 1)) = 0.3
        _OuterGlow ("Outer Glow", Range(0, 0.05)) = 0.01
        
        [Header(Scanlines)]
        _ScanlineEnabled ("Enable Scanlines", Range(0, 1)) = 1
        _ScanlineFrequency ("Scanline Frequency", Range(0, 500)) = 150
        _ScanlineSpeed ("Scanline Speed", Range(-5, 5)) = 1
        _ScanlineIntensity ("Scanline Intensity", Range(0, 0.3)) = 0.05
        
        [Header(Flicker)]
        _FlickerEnabled ("Enable Flicker", Range(0, 1)) = 1
        _FlickerSpeed ("Flicker Speed", Range(0, 20)) = 8
        _FlickerIntensity ("Flicker Intensity", Range(0, 0.1)) = 0.02
        
        [Header(Subtle Glitch)]
        _GlitchEnabled ("Enable Glitch", Range(0, 1)) = 1
        _GlitchFrequency ("Glitch Frequency", Range(0, 0.02)) = 0.003
        _GlitchIntensity ("Glitch Intensity", Range(0, 0.05)) = 0.01
        _GlitchSpeed ("Glitch Speed", Range(0, 5)) = 1
        
        [Header(Character Dropout)]
        _CharDropoutEnabled ("Enable Char Dropout", Range(0, 1)) = 1
        _CharDropoutFrequency ("Dropout Frequency", Range(0, 0.1)) = 0.02
        _CharDropoutSpeed ("Dropout Speed", Range(0, 10)) = 3
        _CharDropoutDuration ("Dropout Duration", Range(0.1, 2)) = 0.3
        
        [Header(Distortion)]
        _DistortionEnabled ("Enable Distortion", Range(0, 1)) = 1
        _DistortionAmount ("Distortion Amount", Range(0, 0.02)) = 0.003
        _DistortionSpeed ("Distortion Speed", Range(0, 3)) = 0.5
        
        [Header(Chromatic Aberration)]
        _ChromaticEnabled ("Enable Chromatic", Range(0, 1)) = 1
        _ChromaticAberration ("Chromatic Amount", Range(0, 0.01)) = 0.002
        
        [Header(Noise)]
        _NoiseEnabled ("Enable Noise", Range(0, 1)) = 1
        _NoiseIntensity ("Noise Intensity", Range(0, 0.1)) = 0.03
        _NoiseSpeed ("Noise Speed", Range(0, 3)) = 0.5
        _NoiseScale ("Noise Scale", Range(10, 200)) = 80
        
        [Header(Brightness)]
        _BaseBrightness ("Base Brightness", Range(0.5, 1.5)) = 1
        _BrightnessVariation ("Brightness Variation", Range(0, 0.2)) = 0.05
        
        [Header(Bloom)]
        _BloomEnabled ("Enable Bloom", Range(0, 1)) = 1
        _BloomStrength ("Bloom Strength", Range(0, 1)) = 0.3
        _BloomSize ("Bloom Size", Range(0, 0.02)) = 0.005
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
            float _InnerGlow;
            float _OuterGlow;
            
            float _ScanlineEnabled;
            float _ScanlineFrequency;
            float _ScanlineSpeed;
            float _ScanlineIntensity;
            
            float _FlickerEnabled;
            float _FlickerSpeed;
            float _FlickerIntensity;
            
            float _GlitchEnabled;
            float _GlitchFrequency;
            float _GlitchIntensity;
            float _GlitchSpeed;
            
            float _CharDropoutEnabled;
            float _CharDropoutFrequency;
            float _CharDropoutSpeed;
            float _CharDropoutDuration;
            
            float _DistortionEnabled;
            float _DistortionAmount;
            float _DistortionSpeed;
            
            float _ChromaticEnabled;
            float _ChromaticAberration;
            
            float _NoiseEnabled;
            float _NoiseIntensity;
            float _NoiseSpeed;
            float _NoiseScale;
            
            float _BaseBrightness;
            float _BrightnessVariation;
            
            float _BloomEnabled;
            float _BloomStrength;
            float _BloomSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            float random(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y;
                
                if (_CharDropoutEnabled > 0.5)
                {
                    float charGrid = floor(uv.x * 30.0) + floor(uv.y * 10.0) * 30.0;
                    float dropoutTime = time * _CharDropoutSpeed;
                    float dropoutPhase = frac(dropoutTime + random(float2(charGrid, 0)));
                    float dropoutChance = random(float2(charGrid, floor(dropoutTime)));
                    
                    if (dropoutChance < _CharDropoutFrequency && dropoutPhase < _CharDropoutDuration)
                    {
                        return fixed4(0, 0, 0, 0);
                    }
                }
                
                if (_DistortionEnabled > 0.5)
                {
                    float distort = sin(uv.y * 15.0 + time * _DistortionSpeed) * _DistortionAmount;
                    uv.x += distort * _DistortionEnabled;
                }
                
                if (_GlitchEnabled > 0.5)
                {
                    float glitchChance = step(1.0 - _GlitchFrequency, random(float2(floor(uv.y * 20.0), floor(time * _GlitchSpeed))));
                    float glitchOffset = (random(float2(floor(uv.y * 20.0), floor(time * _GlitchSpeed))) - 0.5) * _GlitchIntensity;
                    uv.x += glitchChance * glitchOffset * _GlitchEnabled;
                }
                
                fixed4 col = tex2D(_MainTex, uv);
                float alpha = col.a;
                
                if (_ChromaticEnabled > 0.5 && alpha > 0.1)
                {
                    float2 offsetR = uv + float2(_ChromaticAberration, 0);
                    float2 offsetB = uv - float2(_ChromaticAberration, 0);
                    float r = tex2D(_MainTex, offsetR).a;
                    float g = alpha;
                    float b = tex2D(_MainTex, offsetB).a;
                    alpha = max(max(r, g), b);
                }
                
                float bloom = 0.0;
                if (_BloomEnabled > 0.5 && alpha > 0.1)
                {
                    for (float x = -1.0; x <= 1.0; x += 1.0)
                    {
                        for (float y = -1.0; y <= 1.0; y += 1.0)
                        {
                            float2 offset = float2(x, y) * _BloomSize;
                            bloom += tex2D(_MainTex, uv + offset).a;
                        }
                    }
                    bloom = (bloom / 9.0) * _BloomStrength * _BloomEnabled;
                }
                
                float brightness = _BaseBrightness;
                
                if (_ScanlineEnabled > 0.5)
                {
                    float scanline = sin(uv.y * _ScanlineFrequency + time * _ScanlineSpeed);
                    scanline = scanline * _ScanlineIntensity;
                    brightness += scanline * _ScanlineEnabled;
                }
                
                if (_FlickerEnabled > 0.5)
                {
                    float flicker = sin(time * _FlickerSpeed) * _FlickerIntensity;
                    brightness += flicker * _FlickerEnabled;
                }
                
                if (_NoiseEnabled > 0.5)
                {
                    float noiseVal = noise(uv * _NoiseScale + time * _NoiseSpeed);
                    noiseVal = (noiseVal - 0.5) * _NoiseIntensity;
                    brightness += noiseVal * _NoiseEnabled;
                }
                
                brightness = clamp(brightness, _BaseBrightness - _BrightnessVariation, _BaseBrightness + _BrightnessVariation);
                
                float finalAlpha = alpha + bloom;
                float glow = lerp(1.0, _GlowIntensity, alpha);
                
                col.rgb = _Color.rgb * brightness * glow;
                col.a = finalAlpha * _Color.a * i.color.a;
                
                return col;
            }
            ENDCG
        }
    }
}