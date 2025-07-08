Shader "Hidden/Alkahest/BrushPainter"
{
    Properties
    {
        _MainTex ("Splatmap", 2D) = "white" {}
        _BrushMask ("Brush Mask", 2D) = "white" {}
        _LayerIndex ("Layer Index", Float) = 0
        _BrushStrength ("Brush Strength", Float) = 1
        _BrushRotation ("Brush Rotation", Float) = 0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _BrushMask;
            float _LayerIndex;
            float _BrushStrength;
            float _BrushRotation;

            fixed4 frag(v2f_img i) : SV_Target
            {
                // Read existing splatmap color
                fixed4 splat = tex2D(_MainTex, i.uv);

                // Rotate brush UV around center (0.5, 0.5)
                float angleRad = radians(_BrushRotation);
                float2 centeredUV = i.uv - 0.5;
                float cosA = cos(angleRad);
                float sinA = sin(angleRad);

                float2 rotatedUV = float2(
                    cosA * centeredUV.x - sinA * centeredUV.y,
                    sinA * centeredUV.x + cosA * centeredUV.y
                ) + 0.5;

                // Sample brush mask at rotated UV
                fixed brushVal = tex2D(_BrushMask, rotatedUV).r * _BrushStrength;

                // Apply to correct channel
                if (_LayerIndex == 0) splat.r = saturate(splat.r + brushVal);
                if (_LayerIndex == 1) splat.g = saturate(splat.g + brushVal);
                if (_LayerIndex == 2) splat.b = saturate(splat.b + brushVal);
                if (_LayerIndex == 3) splat.a = saturate(splat.a + brushVal);

                return splat;
            }
            ENDCG
        }
    }
}
