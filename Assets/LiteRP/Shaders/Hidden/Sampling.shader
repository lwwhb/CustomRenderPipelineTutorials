Shader "Hidden/LiteRenderPipeline/Sampling"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "LiteRenderPipeline"}
        LOD 100

        // 0 - Downsample - Box filtering
        Pass
        {
            Name "BoxDownsample"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragBoxDownsample

            #include "../../Runtime/ShaderLibrary/SrpCoreShaderLibraryIncludes.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            SAMPLER(sampler_BlitTexture);

            float _SampleOffset;

            half4 FragBoxDownsample(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float4 d = _BlitTexture_TexelSize.xyxy * float4(-_SampleOffset, -_SampleOffset, _SampleOffset, _SampleOffset);

                half4 s;
                s =  SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + d.xy);
                s += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + d.zy);
                s += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + d.xw);
                s += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + d.zw);

                return s * 0.25h;
            }
            ENDHLSL
        }
    }
}
