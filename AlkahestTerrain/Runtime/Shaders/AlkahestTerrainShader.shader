Shader "Alkahest/TerrainBlend"
{
    Properties
    {
        _Splatmap ("Control (RGBA)", 2D) = "white" {}

        _Layer0Albedo ("Layer 0 Albedo", 2D) = "white" {}
        _Layer0Normal ("Layer 0 Normal", 2D) = "bump" {}
        _Layer0Tiling ("Layer 0 Tiling", Vector) = (1,1,0,0)
        _Layer0Metallic ("Layer 0 Metallic", Range(0,1)) = 0
        _Layer0Smoothness ("Layer 0 Smoothness", Range(0,1)) = 0.5

        _Layer1Albedo ("Layer 1 Albedo", 2D) = "white" {}
        _Layer1Normal ("Layer 1 Normal", 2D) = "bump" {}
        _Layer1Tiling ("Layer 1 Tiling", Vector) = (1,1,0,0)
        _Layer1Metallic ("Layer 1 Metallic", Range(0,1)) = 0
        _Layer1Smoothness ("Layer 1 Smoothness", Range(0,1)) = 0.5

        _Layer2Albedo ("Layer 2 Albedo", 2D) = "white" {}
        _Layer2Normal ("Layer 2 Normal", 2D) = "bump" {}
        _Layer2Tiling ("Layer 2 Tiling", Vector) = (1,1,0,0)
        _Layer2Metallic ("Layer 2 Metallic", Range(0,1)) = 0
        _Layer2Smoothness ("Layer 2 Smoothness", Range(0,1)) = 0.5

        _Layer3Albedo ("Layer 3 Albedo", 2D) = "white" {}
        _Layer3Normal ("Layer 3 Normal", 2D) = "bump" {}
        _Layer3Tiling ("Layer 3 Tiling", Vector) = (1,1,0,0)
        _Layer3Metallic ("Layer 3 Metallic", Range(0,1)) = 0
        _Layer3Smoothness ("Layer 3 Smoothness", Range(0,1)) = 0.5

        _FlowMap ("Flow Map", 2D) = "gray" {}
        _FlowMapScale ("Flow Map Scale", Float) = 1024
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _Splatmap;

        sampler2D _Layer0Albedo, _Layer1Albedo, _Layer2Albedo, _Layer3Albedo;
        sampler2D _Layer0Normal, _Layer1Normal, _Layer2Normal, _Layer3Normal;
        float4 _Layer0Tiling, _Layer1Tiling, _Layer2Tiling, _Layer3Tiling;
        float _Layer0Metallic, _Layer1Metallic, _Layer2Metallic, _Layer3Metallic;
        float _Layer0Smoothness, _Layer1Smoothness, _Layer2Smoothness, _Layer3Smoothness;

        sampler2D _FlowMap;
        float _FlowMapScale;

        struct Input
        {
            float3 worldPos;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.worldPos.xz;

            float4 splat = tex2D(_Splatmap, uv / _FlowMapScale);
            splat = max(splat, 0);
            float total = splat.r + splat.g + splat.b + splat.a + 1e-5;
            splat /= total;

            float maxWeight = splat.r;
            int dominantIndex = 0;
            if (splat.g > maxWeight) { maxWeight = splat.g; dominantIndex = 1; }
            if (splat.b > maxWeight) { maxWeight = splat.b; dominantIndex = 2; }
            if (splat.a > maxWeight) { maxWeight = splat.a; dominantIndex = 3; }

            float flowVal = tex2D(_FlowMap, uv / _FlowMapScale).r;
            float angleRad = flowVal * 6.2831;
            float2x2 rotMatrix = float2x2(cos(angleRad), -sin(angleRad), sin(angleRad), cos(angleRad));

            float2 uv0 = uv * _Layer0Tiling.xy;
            float2 uv1 = uv * _Layer1Tiling.xy;
            float2 uv2 = uv * _Layer2Tiling.xy;
            float2 uv3 = uv * _Layer3Tiling.xy;

            if (dominantIndex == 0) uv0 = mul(rotMatrix, uv0 - 0.5) + 0.5;
            if (dominantIndex == 1) uv1 = mul(rotMatrix, uv1 - 0.5) + 0.5;
            if (dominantIndex == 2) uv2 = mul(rotMatrix, uv2 - 0.5) + 0.5;
            if (dominantIndex == 3) uv3 = mul(rotMatrix, uv3 - 0.5) + 0.5;

            fixed4 col0 = tex2D(_Layer0Albedo, uv0);
            fixed4 col1 = tex2D(_Layer1Albedo, uv1);
            fixed4 col2 = tex2D(_Layer2Albedo, uv2);
            fixed4 col3 = tex2D(_Layer3Albedo, uv3);

            fixed3 nrm0 = UnpackNormal(tex2D(_Layer0Normal, uv0));
            fixed3 nrm1 = UnpackNormal(tex2D(_Layer1Normal, uv1));
            fixed3 nrm2 = UnpackNormal(tex2D(_Layer2Normal, uv2));
            fixed3 nrm3 = UnpackNormal(tex2D(_Layer3Normal, uv3));

            o.Albedo = col0.rgb * splat.r +
                       col1.rgb * splat.g +
                       col2.rgb * splat.b +
                       col3.rgb * splat.a;

            fixed3 blendedNormal = nrm0 * splat.r +
                                   nrm1 * splat.g +
                                   nrm2 * splat.b +
                                   nrm3 * splat.a;
            o.Normal = normalize(blendedNormal);

            o.Metallic = _Layer0Metallic * splat.r +
                         _Layer1Metallic * splat.g +
                         _Layer2Metallic * splat.b +
                         _Layer3Metallic * splat.a;

            o.Smoothness = _Layer0Smoothness * splat.r +
                           _Layer1Smoothness * splat.g +
                           _Layer2Smoothness * splat.b +
                           _Layer3Smoothness * splat.a;

            o.Alpha = 1;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
