Shader "Hidden/LiteRenderPipeline/FallbackError"
{
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "LiteRenderPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Error"
            
            HLSLPROGRAM
            #pragma target 2.0
            #pragma editor_sync_compilation

            // -------------------------------------
            // Shader Stages
            #pragma vertex vert
            #pragma fragment frag
            
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "../Runtime/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "../Runtime/ShaderLibrary/SrpCoreShaderLibraryIncludes.hlsl"
            #include "../Runtime/ShaderLibrary/ShaderVariablesInput.hlsl"

            struct appdata_t
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return float4(1,0,1,1);
            }
            ENDHLSL
        }
    }

    Fallback "Hidden/Core/FallbackError"
}
