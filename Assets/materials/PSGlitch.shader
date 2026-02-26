Shader "Custom/PSGlitch"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0, 0.2, 0, 1)
        _MediumColor ("Medium Color", Color) = (0, 0.5, 0, 1)
        _BrightColor ("Bright Color", Color) = (0, 1, 0, 1)
        
        [Header(Background)]
        _BackgroundType ("Background Type", Range(0, 3)) = 0
        _BackgroundColor ("Background Color", Color) = (0, 0.1, 0, 1)
        _BackgroundPixelation ("Background Pixelation", Range(1, 200)) = 1
        _BackgroundNoiseScale ("Background Noise Scale", Range(0.1, 10)) = 1
        _BackgroundNoiseSpeed ("Background Noise Speed", Range(0, 2)) = 0.1
        
        [Header(Scanlines)]
        _ScanlineType ("Scanline Type", Range(0, 2)) = 0
        _ScanlineFrequency ("Scanline Frequency", Range(0, 200)) = 100
        _ScanlineSpeed ("Scanline Speed", Range(-10, 10)) = 5
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.1
        
        [Header(Glitch)]
        _GlitchFrequency ("Glitch Frequency", Range(0, 0.1)) = 0.01
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.1
        _GlitchSpeed ("Glitch Speed", Range(0, 10)) = 1
        _GlitchBlockSize ("Glitch Block Size", Range(1, 100)) = 10
        
        [Header(Noise)]
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.1
        _NoiseSpeed ("Noise Speed", Range(0, 5)) = 0.1
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 1
        
        [Header(Flicker)]
        _FlickerSpeed ("Flicker Speed", Range(0, 20)) = 5
        _FlickerIntensity ("Flicker Intensity", Range(0, 0.5)) = 0.05
        
        [Header(Color Aberration)]
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.05)) = 0.01
        
        [Header(Brightness)]
        _MinBrightness ("Min Brightness", Range(0, 1)) = 0.2
        _MaxBrightness ("Max Brightness", Range(0, 2)) = 1.0
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

            float4 _BaseColor;
            float4 _MediumColor;
            float4 _BrightColor;
            
            float _BackgroundType;
            float4 _BackgroundColor;
            float _BackgroundPixelation;
            float _BackgroundNoiseScale;
            float _BackgroundNoiseSpeed;
            
            float _ScanlineType;
            float _ScanlineFrequency;
            float _ScanlineSpeed;
            float _ScanlineIntensity;
            
            float _GlitchFrequency;
            float _GlitchIntensity;
            float _GlitchSpeed;
            float _GlitchBlockSize;
            
            float _NoiseIntensity;
            float _NoiseSpeed;
            float _NoiseScale;
            
            float _FlickerSpeed;
            float _FlickerIntensity;
            
            float _ChromaticAberration;
            
            float _MinBrightness;
            float _MaxBrightness;

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

            fixed4 getBackground(float2 uv, float time)
            {
                float2 bgUV = uv;
                
                if (_BackgroundPixelation > 1.0)
                {
                    bgUV = floor(uv * _BackgroundPixelation) / _BackgroundPixelation;
                }
                
                fixed4 bgColor = _BackgroundColor;
                
                if (_BackgroundType < 0.5)
                {
                    return bgColor;
                }
                else if (_BackgroundType < 1.5)
                {
                    float n = noise(bgUV * _BackgroundNoiseScale + time * _BackgroundNoiseSpeed);
                    return bgColor * (0.8 + n * 0.4);
                }
                else if (_BackgroundType < 2.5)
                {
                    float gridX = step(0.95, frac(bgUV.x * 20.0));
                    float gridY = step(0.95, frac(bgUV.y * 20.0));
                    float grid = max(gridX, gridY);
                    return lerp(bgColor, bgColor * 1.5, grid);
                }
                else
                {
                    float r = random(bgUV * time * 0.01);
                    return bgColor * (0.5 + r * 0.5);
                }
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y;
                
                fixed4 background = getBackground(uv, time);
                
                float scanline = 1.0;
                if (_ScanlineType < 0.5)
                {
                    scanline = sin(uv.y * _ScanlineFrequency + time * _ScanlineSpeed);
                }
                else if (_ScanlineType < 1.5)
                {
                    scanline = sin(uv.x * _ScanlineFrequency + time * _ScanlineSpeed);
                }
                else
                {
                    float vertScan = sin(uv.y * _ScanlineFrequency + time * _ScanlineSpeed);
                    float horzScan = sin(uv.x * _ScanlineFrequency + time * _ScanlineSpeed);
                    scanline = (vertScan + horzScan) * 0.5;
                }
                scanline = scanline * _ScanlineIntensity + (1.0 - _ScanlineIntensity);
                
                float glitchRow = floor(uv.y * _GlitchBlockSize);
                float glitchChance = step(1.0 - _GlitchFrequency, random(float2(glitchRow, floor(time * _GlitchSpeed))));
                float glitchOffset = (random(float2(glitchRow, floor(time * _GlitchSpeed))) - 0.5) * _GlitchIntensity;
                uv.x += glitchChance * glitchOffset;
                
                float noiseVal = noise(uv * _NoiseScale + time * _NoiseSpeed);
                noiseVal = noiseVal * _NoiseIntensity;
                
                float flicker = sin(time * _FlickerSpeed) * _FlickerIntensity + (1.0 - _FlickerIntensity);
                
                float brightness = scanline + noiseVal;
                brightness *= flicker;
                brightness = lerp(_MinBrightness, _MaxBrightness, brightness);
                
                float colorNoise = random(uv * time * 0.01);
                fixed4 col;
                if (colorNoise < 0.7)
                {
                    col = lerp(_BaseColor, _MediumColor, colorNoise / 0.7);
                }
                else if (colorNoise < 0.95)
                {
                    col = _MediumColor;
                }
                else
                {
                    col = _BrightColor;
                }
                
                col *= brightness;
                
                if (glitchChance > 0.5 && random(float2(uv.x, time)) > 0.7)
                {
                    col = _BrightColor;
                }
                
                if (_ChromaticAberration > 0.0)
                {
                    float2 offsetR = uv + float2(_ChromaticAberration, 0);
                    float2 offsetB = uv - float2(_ChromaticAberration, 0);
                    
                    float r = lerp(_BaseColor.r, _BrightColor.r, brightness);
                    float g = col.g;
                    float b = lerp(_BaseColor.b, _BrightColor.b, brightness);
                    
                    col = fixed4(r, g, b, 1);
                }
                
                col = lerp(background, col, col.a);
                
                return col;
            }
            ENDCG
        }
    }
}
