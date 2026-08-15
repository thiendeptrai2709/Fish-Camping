Shader "Skybox/BlendedSkybox"
{
    Properties
    {
        _Blend ("Blend Factor", Range(0, 1)) = 0.0
        [NoScaleOffset] _DaySkybox ("Day Cubemap (HDR)", Cube) = "_Skybox" {}
        [NoScaleOffset] _NightSkybox ("Night Cubemap (HDR)", Cube) = "_Skybox" {}
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            samplerCUBE _DaySkybox;
            half4 _DaySkybox_HDR;
            samplerCUBE _NightSkybox;
            half4 _NightSkybox_HDR;
            half4 _Tint;
            half _Exposure;
            float _Blend;
            float _Rotation;

            float3 RotateAroundYInDegrees(float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            struct appdata_t
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 rotated = RotateAroundYInDegrees(v.vertex.xyz, _Rotation);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = rotated;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half4 dayData = texCUBE(_DaySkybox, i.texcoord);
                half4 nightData = texCUBE(_NightSkybox, i.texcoord);

                half3 dayColor = DecodeHDR(dayData, _DaySkybox_HDR);
                half3 nightColor = DecodeHDR(nightData, _NightSkybox_HDR);

                half3 finalColor = lerp(dayColor, nightColor, _Blend);
                finalColor *= _Tint.rgb * unity_ColorSpaceDouble.rgb * _Exposure;

                return half4(finalColor, 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}