Shader "Unlit/OscilloscopeLine"
{
    Properties
    {
        _Color ("Color", Color) = (0.0, 1.0, 0.0, 1.0)
        _DecayTime ("Decay time", Float) = 0.1
        _Intensity ("Intensity", Float) = 1.0
        _Radius ("Radius", Float) = 0.001
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend One One
            ZTest Always

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 coord_length : TEXCOORD0; // X along the line, Y across
                float2 timeSinceStart_deltaTime : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 coord : TEXCOORD0;
                float timeSinceStart : TEXCOORD1;
                float deltaTime : TEXCOORD2;
                float length : TEXCOORD3;
            };

            #define SQRT2 1.4142135623730951
            #define SQRT2PI 2.506628274631001

            float erf(float x) {
                float s = sign(x);
                float a = abs(x);
                x = 1.0 + (0.278393 + (0.230389 + 0.078108 * a * a) * a) * a;
                x *= x;
                return s - s / (x * x);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.coord = v.coord_length.xy;
                o.timeSinceStart = v.timeSinceStart_deltaTime.x;
                o.deltaTime = v.timeSinceStart_deltaTime.y;
                o.length = v.coord_length.z;
                return o;
            }

            uniform float3 _Color;
            uniform float _Radius;
            uniform float _Intensity;
            uniform float _DecayTime;

            float4 frag (v2f i) : SV_Target
            {
                float sigma = _Radius / 5.0;
                float multiplier = _Intensity * i.deltaTime;

                if(i.length < 1e-5)
                {
                    multiplier *= exp(-dot(i.coord, i.coord) / (2.0 * sigma * sigma)) / (SQRT2PI * sigma);
                    return float4(0.0f, 0.0f, 1.0f, 1.0f);
                } else {
                    float f = i.deltaTime * sigma / (SQRT2 * i.length * _DecayTime);

                    multiplier *= erf(f + i.coord.x / (SQRT2 * sigma)) - erf(f + (i.coord.x - i.length) / (SQRT2 * sigma));

                    multiplier *= exp(
                        f * f -
                        i.coord.y * i.coord.y / (2.0 * sigma * sigma) -
                        (i.timeSinceStart + i.deltaTime * i.coord.x / i.length) / _DecayTime
                    ) / (2.0 * i.length);
                }
                return float4(multiplier * _Color, 1.0);
            }
            ENDCG
        }
    }
}
