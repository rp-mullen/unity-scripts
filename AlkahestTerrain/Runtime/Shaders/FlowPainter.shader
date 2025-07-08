Shader "Hidden/Alkahest/FlowPainter"
{
    Properties
    {
        _MainTex ("Current Flow Map", 2D) = "white" {}
        _BrushMask ("Brush Mask", 2D) = "white" {}
        _BrushRotation ("Brush Rotation", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _BrushMask;
            float _BrushRotation; // normalized 0–1

            fixed4 frag(v2f_img i) : SV_Target
            {
                float currentFlow = tex2D(_MainTex, i.uv).r;
                float brushMask = tex2D(_BrushMask, i.uv).r;

                // Lerp between existing value and new rotation based on brush mask
                float newFlow = lerp(currentFlow, _BrushRotation, brushMask);

                return fixed4(newFlow, newFlow, newFlow, 1);
            }
            ENDCG
        }
    }
}
