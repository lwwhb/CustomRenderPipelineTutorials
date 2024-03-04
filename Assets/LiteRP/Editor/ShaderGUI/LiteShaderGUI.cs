using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Editor
{
    public abstract class LiteShaderGUI : ShaderGUI
    {
        public enum SurfaceType
        {
            Opaque,
            Transparent
        }
        
        public enum BlendMode
        {
            Alpha,   
            Premultiply, 
            Additive,
            Multiply
        }
        internal static class LiteRPShaderProperty
        {
            public static readonly string SurfaceType = "_Surface";
            public static readonly string BlendMode = "_Blend";
            public static readonly string AlphaClip = "_AlphaClip";
            public static readonly string AlphaToMask = "_AlphaToMask";
            public static readonly string SrcBlend = "_SrcBlend";
            public static readonly string DstBlend = "_DstBlend";
            public static readonly string SrcBlendAlpha = "_SrcBlendAlpha";
            public static readonly string DstBlendAlpha = "_DstBlendAlpha";
            public static readonly string BlendModePreserveSpecular = "_BlendModePreserveSpecular";
            public static readonly string ZWrite = "_ZWrite";
            public static readonly string CullMode = "_Cull";
            public static readonly string CastShadows = "_CastShadows";
            public static readonly string ReceiveShadows = "_ReceiveShadows";
            public static readonly string QueueOffset = "_QueueOffset";
        }
        
        protected MaterialEditor m_MaterialEditor { get; set; }
        
        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            if (materialEditorIn == null)
                throw new ArgumentNullException("materialEditorIn");
            
            if (!(RenderPipelineManager.currentPipeline is LiteRenderPipeline))
            {
                CoreEditorUtils.DrawFixMeBox("Editing LiteRP materials is only supported when an LiteRP asset is assigned in the Graphics Settings", MessageType.Warning, "Open",
                    () => SettingsService.OpenProjectSettings("Project/Graphics"));
            }
            else
            {
                m_MaterialEditor = materialEditorIn;
                OnMaterialGUI(materialEditorIn, properties);
            }
        }

        internal static void SetupMaterialBlendMode(Material material)
        {
            bool alphaClip = false;
            if (material.HasProperty(LiteRPShaderProperty.AlphaClip))
                alphaClip = material.GetFloat(LiteRPShaderProperty.AlphaClip) >= 0.5;
            CoreUtils.SetKeyword(material, ShaderKeywordStrings._ALPHATEST_ON, alphaClip);
            int renderQueue = material.shader.renderQueue;
            material.SetOverrideTag("RenderType", "");
            if (material.HasProperty(LiteRPShaderProperty.SurfaceType))
            {
                SurfaceType surfaceType = (SurfaceType)material.GetFloat(LiteRPShaderProperty.SurfaceType);
                bool zwrite = false;
                CoreUtils.SetKeyword(material, ShaderKeywordStrings._SURFACE_TYPE_TRANSPARENT, surfaceType == SurfaceType.Transparent);
                bool alphaToMask = false;
                if (surfaceType == SurfaceType.Opaque)
                {
                    if (alphaClip)
                    {
                        renderQueue = (int)RenderQueue.AlphaTest;
                        material.SetOverrideTag("RenderType", "TransparentCutout");
                        alphaToMask = true;
                    }
                    else
                    {
                        renderQueue = (int)RenderQueue.Geometry;
                        material.SetOverrideTag("RenderType", "Opaque");
                    }

                    SetMaterialSrcDstBlendProperties(material, UnityEngine.Rendering.BlendMode.One, UnityEngine.Rendering.BlendMode.Zero);
                    zwrite = true;
                    material.DisableKeyword(ShaderKeywordStrings._ALPHAPREMULTIPLY_ON);
                    material.DisableKeyword(ShaderKeywordStrings._ALPHAMODULATE_ON);
                }
                else 
                {
                    BlendMode blendMode = (BlendMode)material.GetFloat(LiteRPShaderProperty.BlendMode);

                    var srcBlendRGB = UnityEngine.Rendering.BlendMode.One;
                    var dstBlendRGB = UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                    var srcBlendA = UnityEngine.Rendering.BlendMode.One;
                    var dstBlendA = UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;

                    // 透明模式设置
                    // ref: https://docs.unity3d.com/Manual/SL-Blend.html
                    switch (blendMode)
                    {
                        // srcRGB * srcAlpha + dstRGB * (1 - srcAlpha)
                        // preserve spec: 
                        // srcRGB * (<in shader> ? 1 : srcAlpha) + dstRGB * (1 - srcAlpha)
                        case BlendMode.Alpha:
                            srcBlendRGB = UnityEngine.Rendering.BlendMode.SrcAlpha;
                            dstBlendRGB = UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                            srcBlendA = UnityEngine.Rendering.BlendMode.One;
                            dstBlendA = dstBlendRGB;
                            break;

                        // srcRGB < srcAlpha, (alpha multiplied in asset)
                        // srcRGB * 1 + dstRGB * (1 - srcAlpha)
                        case BlendMode.Premultiply:
                            srcBlendRGB = UnityEngine.Rendering.BlendMode.One;
                            dstBlendRGB = UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                            srcBlendA = srcBlendRGB;
                            dstBlendA = dstBlendRGB;
                            break;

                        // srcRGB * srcAlpha + dstRGB * 1, (alpha controls amount of addition)
                        // preserve spec:
                        // srcRGB * (<in shader> ? 1 : srcAlpha) + dstRGB * (1 - srcAlpha)
                        case BlendMode.Additive:
                            srcBlendRGB = UnityEngine.Rendering.BlendMode.SrcAlpha;
                            dstBlendRGB = UnityEngine.Rendering.BlendMode.One;
                            srcBlendA = UnityEngine.Rendering.BlendMode.One;
                            dstBlendA = dstBlendRGB;
                            break;

                        // srcRGB * 0 + dstRGB * srcRGB
                        // in shader alpha controls amount of multiplication, lerp(1, srcRGB, srcAlpha)
                        // Multiply affects color only, keep existing alpha.
                        case BlendMode.Multiply:
                            srcBlendRGB = UnityEngine.Rendering.BlendMode.DstColor;
                            dstBlendRGB = UnityEngine.Rendering.BlendMode.Zero;
                            srcBlendA = UnityEngine.Rendering.BlendMode.Zero;
                            dstBlendA = UnityEngine.Rendering.BlendMode.One;
                            break;
                    }

                    // Lift alpha multiply from ROP to shader by setting pre-multiplied _SrcBlend mode.
                    // The intent is to do different blending for diffuse and specular in shader.
                    // ref: http://advances.realtimerendering.com/other/2016/naughty_dog/NaughtyDog_TechArt_Final.pdf
                    bool preserveSpecular = (material.HasProperty(LiteRPShaderProperty.BlendModePreserveSpecular) &&
                                             material.GetFloat(LiteRPShaderProperty.BlendModePreserveSpecular) > 0) &&
                                            blendMode != BlendMode.Multiply && blendMode != BlendMode.Premultiply;
                    if (preserveSpecular)
                    {
                        srcBlendRGB = UnityEngine.Rendering.BlendMode.One;
                    }
                    
                    SetMaterialSrcDstBlendProperties(material, srcBlendRGB, dstBlendRGB, // RGB
                        srcBlendA, dstBlendA); // Alpha

                    CoreUtils.SetKeyword(material, ShaderKeywordStrings._ALPHAPREMULTIPLY_ON, preserveSpecular);
                    CoreUtils.SetKeyword(material, ShaderKeywordStrings._ALPHAMODULATE_ON, blendMode == BlendMode.Multiply);

                    // General Transparent Material Settings
                    material.SetOverrideTag("RenderType", "Transparent");
                    zwrite = false;
                    renderQueue = (int)RenderQueue.Transparent;
                }
                
                if (material.HasProperty(LiteRPShaderProperty.AlphaToMask))
                {
                    material.SetFloat(LiteRPShaderProperty.AlphaToMask, alphaToMask ? 1.0f : 0.0f);
                }
                
                SetMaterialZWriteProperty(material, zwrite);
                material.SetShaderPassEnabled("DepthOnly", zwrite);
            }
            else
            {
                material.SetShaderPassEnabled("DepthOnly", true);
            }
            
            // 处理Shader中改变的RenderQueue
            if (material.HasProperty(LiteRPShaderProperty.QueueOffset))
                renderQueue += (int)material.GetFloat(LiteRPShaderProperty.QueueOffset);

            if (renderQueue != material.renderQueue)
                material.renderQueue = renderQueue;
        }

        internal static void SetMaterialSrcDstBlendProperties(Material material, UnityEngine.Rendering.BlendMode srcBlend, UnityEngine.Rendering.BlendMode dstBlend)
        {
            if (material.HasProperty(LiteRPShaderProperty.SrcBlend))
                material.SetFloat(LiteRPShaderProperty.SrcBlend, (float)srcBlend);

            if (material.HasProperty(LiteRPShaderProperty.DstBlend))
                material.SetFloat(LiteRPShaderProperty.DstBlend, (float)dstBlend);

            if (material.HasProperty(LiteRPShaderProperty.SrcBlendAlpha))
                material.SetFloat(LiteRPShaderProperty.SrcBlendAlpha, (float)srcBlend);

            if (material.HasProperty(LiteRPShaderProperty.DstBlendAlpha))
                material.SetFloat(LiteRPShaderProperty.DstBlendAlpha, (float)dstBlend);
        }
        internal static void SetMaterialSrcDstBlendProperties(Material material, UnityEngine.Rendering.BlendMode srcBlendRGB, UnityEngine.Rendering.BlendMode dstBlendRGB, UnityEngine.Rendering.BlendMode srcBlendAlpha, UnityEngine.Rendering.BlendMode dstBlendAlpha)
        {
            if (material.HasProperty(LiteRPShaderProperty.SrcBlend))
                material.SetFloat(LiteRPShaderProperty.SrcBlend, (float)srcBlendRGB);

            if (material.HasProperty(LiteRPShaderProperty.DstBlend))
                material.SetFloat(LiteRPShaderProperty.DstBlend, (float)dstBlendRGB);

            if (material.HasProperty(LiteRPShaderProperty.SrcBlendAlpha))
                material.SetFloat(LiteRPShaderProperty.SrcBlendAlpha, (float)srcBlendAlpha);

            if (material.HasProperty(LiteRPShaderProperty.DstBlendAlpha))
                material.SetFloat(LiteRPShaderProperty.DstBlendAlpha, (float)dstBlendAlpha);
        }
        
        internal static void SetMaterialZWriteProperty(Material material, bool zwriteEnabled)
        {
            if (material.HasProperty(LiteRPShaderProperty.ZWrite))
                material.SetFloat(LiteRPShaderProperty.ZWrite, zwriteEnabled ? 1.0f : 0.0f);
        }

        protected abstract void OnMaterialGUI(MaterialEditor materialEditor, MaterialProperty[] props);
    }
}
