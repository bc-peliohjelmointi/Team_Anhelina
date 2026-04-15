Shader "Custom/RetroTextReadable"
{
    Properties
    {
        _MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Text Color", Color) = (0, 1, 0, 1)
        
        // glow around text characters
        [Header(Glow Effect)]
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1.2
        _InnerGlow ("Inner Glow", Range(0, 1)) = 0.3
        _OuterGlow ("Outer Glow", Range(0, 0.05)) = 0.01
        
        // horizontal lines moving down the screen
        [Header(Scanlines)]
        _ScanlineEnabled ("Enable Scanlines", Range(0, 1)) = 1
        _ScanlineFrequency ("Scanline Frequency", Range(0, 500)) = 150
        _ScanlineSpeed ("Scanline Speed", Range(-5, 5)) = 1
        _ScanlineIntensity ("Scanline Intensity", Range(0, 0.3)) = 0.05
        
        // slight brightness variation over time
        [Header(Flicker)]
        _FlickerEnabled ("Enable Flicker", Range(0, 1)) = 1
        _FlickerSpeed ("Flicker Speed", Range(0, 20)) = 8
        _FlickerIntensity ("Flicker Intensity", Range(0, 0.1)) = 0.02
        
        // occasional horizontal shifts on random rows
        [Header(Subtle Glitch)]
        _GlitchEnabled ("Enable Glitch", Range(0, 1)) = 1
        _GlitchFrequency ("Glitch Frequency", Range(0, 0.02)) = 0.003
        _GlitchIntensity ("Glitch Intensity", Range(0, 0.05)) = 0.01
        _GlitchSpeed ("Glitch Speed", Range(0, 5)) = 1
        
        // makes individual characters blink out randomly
        [Header(Character Dropout)]
        _CharDropoutEnabled ("Enable Char Dropout", Range(0, 1)) = 1
        _CharDropoutFrequency ("Dropout Frequency", Range(0, 0.1)) = 0.02
        _CharDropoutSpeed ("Dropout Speed", Range(0, 10)) = 3
        _CharDropoutDuration ("Dropout Duration", Range(0.1, 2)) = 0.3
        
        // wavy distortion on text rows
        [Header(Distortion)]
        _DistortionEnabled ("Enable Distortion", Range(0, 1)) = 1
        _DistortionAmount ("Distortion Amount", Range(0, 0.02)) = 0.003
        _DistortionSpeed ("Distortion Speed", Range(0, 3)) = 0.5
        
        // red/blue channel offset for old CRT look
        [Header(Chromatic Aberration)]
        _ChromaticEnabled ("Enable Chromatic", Range(0, 1)) = 1
        _ChromaticAberration ("Chromatic Amount", Range(0, 0.01)) = 0.002
        
        // random pixel variation
        [Header(Noise)]
        _NoiseEnabled ("Enable Noise", Range(0, 1)) = 1
        _NoiseIntensity ("Noise Intensity", Range(0, 0.1)) = 0.03
        _NoiseSpeed ("Noise Speed", Range(0, 3)) = 0.5
        _NoiseScale ("Noise Scale", Range(10, 200)) = 80
        
        [Header(Brightness)]
        _BaseBrightness ("Base Brightness", Range(0.5, 1.5)) = 1
        _BrightnessVariation ("Brightness Variation", Range(0, 0.2)) = 0.05
        
        // soft glow bleeding around characters
        [Header(Bloom)]
        _BloomEnabled ("Enable Bloom", Range(0, 1)) = 1
        _BloomStrength ("Bloom Strength", Range(0, 1)) = 0.3
        _BloomSize ("Bloom Size", Range(0, 0.02)) = 0.005
    }
    
    SubShader
    {
        // transparent because font texture has alpha
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

            // basic hash function for pseudo-random values
            float random(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            // smooth noise using bilinear interpolation
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f); // smoothstep
                
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
                
                // character dropout - randomly hides whole character cells
                if (_CharDropoutEnabled > 0.5)
                {
                    float charGrid = floor(uv.x * 30.0) + floor(uv.y * 10.0) * 30.0;
                    float dropoutTime = time * _CharDropoutSpeed;
                    float dropoutPhase = frac(dropoutTime + random(float2(charGrid, 0)));
                    float dropoutChance = random(float2(charGrid, floor(dropoutTime)));
                    
                    if (dropoutChance < _CharDropoutFrequency && dropoutPhase < _CharDropoutDuration)
                    {
                        return fixed4(0, 0, 0, 0); // fully transparent = character dropped out
                    }
                }
                
                // wave distortion on each row
                if (_DistortionEnabled > 0.5)
                {
                    float distort = sin(uv.y * 15.0 + time * _DistortionSpeed) * _DistortionAmount;
                    uv.x += distort * _DistortionEnabled;
                }
                
                // random horizontal shift on occasional rows
                if (_GlitchEnabled > 0.5)
                {
                    float glitchChance = step(1.0 - _GlitchFrequency, random(float2(floor(uv.y * 20.0), floor(time * _GlitchSpeed))));
                    float glitchOffset = (random(float2(floor(uv.y * 20.0), floor(time * _GlitchSpeed))) - 0.5) * _GlitchIntensity;
                    uv.x += glitchChance * glitchOffset * _GlitchEnabled;
                }
                
                fixed4 col = tex2D(_MainTex, uv);
                float alpha = col.a;
                
                // chromatic aberration - slightly offset red and blue channels
                if (_ChromaticEnabled > 0.5 && alpha > 0.1)
                {
                    float2 offsetR = uv + float2(_ChromaticAberration, 0);
                    float2 offsetB = uv - float2(_ChromaticAberration, 0);
                    float r = tex2D(_MainTex, offsetR).a;
                    float g = alpha;
                    float b = tex2D(_MainTex, offsetB).a;
                    alpha = max(max(r, g), b); // take brightest to widen character slightly
                }
                
                // simple 3x3 box blur for bloom effect
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
                
                // sine wave scanlines
                if (_ScanlineEnabled > 0.5)
                {
                    float scanline = sin(uv.y * _ScanlineFrequency + time * _ScanlineSpeed);
                    scanline = scanline * _ScanlineIntensity;
                    brightness += scanline * _ScanlineEnabled;
                }
                
                // slow brightness pulse
                if (_FlickerEnabled > 0.5)
                {
                    float flicker = sin(time * _FlickerSpeed) * _FlickerIntensity;
                    brightness += flicker * _FlickerEnabled;
                }
                
                // add noise variation to brightness
                if (_NoiseEnabled > 0.5)
                {
                    float noiseVal = noise(uv * _NoiseScale + time * _NoiseSpeed);
                    noiseVal = (noiseVal - 0.5) * _NoiseIntensity;
                    brightness += noiseVal * _NoiseEnabled;
                }
                
                // clamp brightness to a reasonable range
                brightness = clamp(brightness, _BaseBrightness - _BrightnessVariation, _BaseBrightness + _BrightnessVariation);
                
                float finalAlpha = alpha + bloom;
                float glow = lerp(1.0, _GlowIntensity, alpha); // brighter where text is solid
                
                col.rgb = _Color.rgb * brightness * glow;
                col.a = finalAlpha * _Color.a * i.color.a;
                
                return col;
            }
            ENDCG
        }
    }
}