Shader "Hidden/LiteRenderPipeline/CopyDepth"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "LiteRenderPipeline"}

        Pass
        {
            Name "CopyDepth"
            ZTest Always
            ZWrite Off
            ColorMask R
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "../../Runtime/ShaderLibrary/CopyDepthPass.hlsl"

            ENDHLSL
        }
    }
}
