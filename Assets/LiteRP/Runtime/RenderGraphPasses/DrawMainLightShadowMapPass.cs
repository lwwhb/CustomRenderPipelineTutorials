using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawMainLightShadowMapProfilingSampler = new ProfilingSampler("DrawMainLightShadowMapPass");
        private const string k_MainLightShadowmapTextureName = "_MainLightShadowmapTexture";
        private const string k_EmptyShadowMapName = "_EmptyLightShadowmapTexture";
        private const int k_MaxCascades = 4;
        private const int k_ShadowmapBufferBits = 16;
        
        
        private TextureHandle m_MainLightShadowHandle = TextureHandle.nullHandle;
        private RTHandle m_MainLightShadowmapTexture = null;
        private RTHandle m_EmptyLightShadowmapTexture = null;
        
        Matrix4x4[] m_MainLightShadowMatrices;
        ShadowSliceData[] m_CascadeSlices;
        Vector4[] m_CascadeSplitDistances;
        
        
        internal class DrawMainLightShadowMapPassData
        {
            internal int mainLightIndex;
            internal Vector3 worldSpaceCameraPos;
            internal ShadowData shadowData;
            
            internal RendererListHandle[] shadowRendererListsHandle = new RendererListHandle[k_MaxCascades];
        }

        void Clear()
        {
            for (int i = 0; i < m_MainLightShadowMatrices.Length; ++i)
                m_MainLightShadowMatrices[i] = Matrix4x4.identity;

            for (int i = 0; i < m_CascadeSplitDistances.Length; ++i)
                m_CascadeSplitDistances[i] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

            for (int i = 0; i < m_CascadeSlices.Length; ++i)
                m_CascadeSlices[i].Clear();
        }
        
        private void InitializeMainLightShadowMapPass()
        {
            m_MainLightShadowMatrices = new Matrix4x4[k_MaxCascades + 1];
            m_CascadeSlices = new ShadowSliceData[k_MaxCascades];
            m_CascadeSplitDistances = new Vector4[k_MaxCascades];
            
            m_EmptyLightShadowmapTexture = ShadowUtils.AllocShadowRT(1, 1, k_ShadowmapBufferBits, 1, 0, name: k_EmptyShadowMapName);
        }

        private bool NeedMainLightShadowPass(CameraData cameraData, LightData lightData, ShadowData shadowData)
        {
            // 判断管线设置中是否开启
            if (!shadowData.mainLightShadowEnabled)
                return false;

            // 判断硬件是否支持
            if (!shadowData.supportMainLightShadow)
                return false;
            
            // 判断场景中是否有光源
            int shadowLightIndex = lightData.mainLightIndex;
            if (shadowLightIndex == -1)
                return false;
            
            // 判断主光源是否开启阴影投射
            VisibleLight shadowLight = lightData.visibleLights[shadowLightIndex];
            Light light = shadowLight.light;
            if (light.shadows == LightShadows.None)
                return false;
            
            // 只有主光源是方向光才开启主光源阴影渲染
            if (shadowLight.lightType != LightType.Directional)
                return false;
            
            // 获取ShadowCaster包裹盒，如果失败则返回false
            Bounds bounds;
            if (!cameraData.cullingResults.GetShadowCasterBounds(shadowLightIndex, out bounds))
                return false;
            
            Clear();
            ref readonly LightShadowCullingInfos shadowCullingInfos = ref shadowData.visibleLightsShadowCullingInfos.UnsafeElementAt(shadowLightIndex);
            for (int cascadeIndex = 0; cascadeIndex < shadowData.mainLightShadowCascadesCount; ++cascadeIndex)
            {
                if (shadowCullingInfos.IsSliceValid(cascadeIndex))
                {
                    ref readonly ShadowSliceData sliceData = ref shadowCullingInfos.slices.UnsafeElementAt(cascadeIndex);
                    m_CascadeSplitDistances[cascadeIndex] = sliceData.splitData.cullingSphere;
                    m_CascadeSlices[cascadeIndex] = sliceData;
                }
            }
            ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_MainLightShadowmapTexture, shadowData.mainLightRenderTargetWidth,
                shadowData.mainLightRenderTargetHeight, k_ShadowmapBufferBits, name: k_MainLightShadowmapTextureName);

            return true;
        }

        private void ReleaseMainLightShadowMapPass()
        {
            m_MainLightShadowmapTexture?.Release();
            m_EmptyLightShadowmapTexture?.Release();
        }
        
        private void AddDrawMainLightShadowPass(RenderGraph renderGraph, CameraData cameraData, LightData lightData, ShadowData shadowData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DrawMainLightShadowMapPassData>("Draw Main Light ShadowMap Pass", out var passData,
                       s_DrawMainLightShadowMapProfilingSampler))
            {
                //设置PassData
                passData.mainLightIndex = lightData.mainLightIndex;
                passData.worldSpaceCameraPos = cameraData.camera.transform.position;
                passData.shadowData = shadowData;
                
                //创建RendererList
                var settings = new ShadowDrawingSettings(cameraData.cullingResults, passData.mainLightIndex);
                settings.useRenderingLayerMaskTest = false; //临时代码
                for (int cascadeIndex = 0; cascadeIndex < shadowData.mainLightShadowCascadesCount; ++cascadeIndex)
                {
                    passData.shadowRendererListsHandle[cascadeIndex] = renderGraph.CreateShadowRendererList(ref settings);
                    builder.UseRendererList(passData.shadowRendererListsHandle[cascadeIndex]);
                }
                
                m_MainLightShadowHandle = LiteRPRenderGraphUtils.CreateRenderGraphTexture(renderGraph,
                    m_MainLightShadowmapTexture.rt.descriptor, k_MainLightShadowmapTextureName, true,
                    ShadowUtils.m_ForceShadowPointSampling ? FilterMode.Point : FilterMode.Bilinear);
                builder.SetRenderAttachmentDepth(m_MainLightShadowHandle, AccessFlags.Write);
                
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);

                if (m_MainLightShadowHandle.IsValid())
                    builder.SetGlobalTextureAfterPass(m_MainLightShadowHandle, ShaderPropertyId.mainLightShadowmap);
                
                builder.SetRenderFunc((DrawMainLightShadowMapPassData data, RasterGraphContext context) =>
                {
                    int shadowLightIndex = data.mainLightIndex;
                    if (shadowLightIndex == -1)
                        return;
                    VisibleLight shadowLight = lightData.visibleLights[shadowLightIndex];
                    
                    context.cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, data.worldSpaceCameraPos);
                    for (int cascadeIndex = 0; cascadeIndex < data.shadowData.mainLightShadowCascadesCount; ++cascadeIndex)
                    {
                        var shadowSliceData = m_CascadeSlices[cascadeIndex];
                        Vector4 shadowBias = ShadowUtils.GeMainLightShadowBias(ref shadowLight, data.shadowData.mainLightShadowBias, data.shadowData.supportsSoftShadows, shadowSliceData.projectionMatrix, shadowSliceData.resolution);
                        context.cmd.SetGlobalVector(ShaderPropertyId.shadowBias, shadowBias);

                        // Light direction is currently used in shadow caster pass to apply shadow normal offset (normal bias).
                        Vector3 lightDirection = -shadowLight.localToWorldMatrix.GetColumn(2);
                        context.cmd.SetGlobalVector(ShaderPropertyId.lightDirection, new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, 0.0f));

                        // For punctual lights, computing light direction at each vertex position provides more consistent results (shadow shape does not change when "rotating the point light" for example)
                        Vector3 lightPosition = shadowLight.localToWorldMatrix.GetColumn(3);
                        context.cmd.SetGlobalVector(ShaderPropertyId.lightPosition, new Vector4(lightPosition.x, lightPosition.y, lightPosition.z, 1.0f));
                        
                        // 绘制Shadow RenderList
                        RendererListHandle shadowRendererListHandle = data.shadowRendererListsHandle[cascadeIndex];
                        context.cmd.SetGlobalDepthBias(1.0f, 2.5f); // these values match HDRP defaults (see https://github.com/Unity-Technologies/Graphics/blob/9544b8ed2f98c62803d285096c91b44e9d8cbc47/com.unity.render-pipelines.high-definition/Runtime/Lighting/Shadow/HDShadowAtlas.cs#L197 )
                        context.cmd.SetViewport(new Rect(shadowSliceData.offsetX, shadowSliceData.offsetY, shadowSliceData.resolution, shadowSliceData.resolution));
                        context.cmd.SetViewProjectionMatrices(shadowSliceData.viewMatrix, shadowSliceData.projectionMatrix);
                        if(shadowRendererListHandle.IsValid())
                            context.cmd.DrawRendererList(shadowRendererListHandle);
                        context.cmd.DisableScissorRect();
                        context.cmd.SetGlobalDepthBias(0.0f, 0.0f); // Restore previous depth bias values
                    }
                    
                    // 设置阴影Shader关键字
                    bool isKeywordSoftShadowsEnabled = shadowLight.light.shadows == LightShadows.Soft && data.shadowData.supportsSoftShadows;
                    context.cmd.SetKeyword(ShaderGlobalKeywords.MainLightShadows, data.shadowData.mainLightShadowCascadesCount == 1);
                    context.cmd.SetKeyword(ShaderGlobalKeywords.MainLightShadowCascades, data.shadowData.mainLightShadowCascadesCount > 1);
                    
                    context.cmd.SetKeyword(ShaderGlobalKeywords.SoftShadows, isKeywordSoftShadowsEnabled);
                    if (isKeywordSoftShadowsEnabled && LiteRPUtils.asset?.softShadowQuality == SoftShadowQuality.Low)
                    {
                        context.cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsLow, true);
                        context.cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsMedium, false);
                        context.cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsHigh, false);
                        context.cmd.SetKeyword(ShaderGlobalKeywords.SoftShadows, false);
                    }
                    else if (isKeywordSoftShadowsEnabled && LiteRPUtils.asset?.softShadowQuality == SoftShadowQuality.Medium)
                    {
                        context.cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsLow, false);
                        context.cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsMedium, true);
                        context.cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsHigh, false);
                        context.cmd.SetKeyword(ShaderGlobalKeywords.SoftShadows, false);
                    }
                    else if (isKeywordSoftShadowsEnabled && LiteRPUtils.asset?.softShadowQuality == SoftShadowQuality.High)
                    {
                        context.cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsLow, false);
                        context.cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsMedium, false);
                        context.cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsHigh, true);
                        context.cmd.SetKeyword(ShaderGlobalKeywords.SoftShadows, false);
                    }

                    SetupMainLightShadowReceiverConstants(context.cmd, ref shadowLight, data.shadowData);
                });
            }
        }
        
        private void SetupMainLightShadowReceiverConstants(RasterCommandBuffer cmd, ref VisibleLight shadowLight, ShadowData shadowData)
        {
            Light light = shadowLight.light;
            bool softShadows = shadowLight.light.shadows == LightShadows.Soft && shadowData.supportsSoftShadows;

            int cascadeCount = shadowData.mainLightShadowCascadesCount;
            for (int i = 0; i < cascadeCount; ++i)
                m_MainLightShadowMatrices[i] = m_CascadeSlices[i].shadowTransform;

            // We setup and additional a no-op WorldToShadow matrix in the last index
            // because the ComputeCascadeIndex function in Shadows.hlsl can return an index
            // out of bounds. (position not inside any cascade) and we want to avoid branching
            Matrix4x4 noOpShadowMatrix = Matrix4x4.zero;
            noOpShadowMatrix.m22 = (SystemInfo.usesReversedZBuffer) ? 1.0f : 0.0f;
            for (int i = cascadeCount; i <= k_MaxCascades; ++i)
                m_MainLightShadowMatrices[i] = noOpShadowMatrix;

            int renderTargetWidth = shadowData.mainLightRenderTargetWidth;
            int renderTargetHeight = shadowData.mainLightRenderTargetHeight;
            float invShadowAtlasWidth = 1.0f / renderTargetWidth;
            float invShadowAtlasHeight = 1.0f / renderTargetHeight;
            float invHalfShadowAtlasWidth = 0.5f * invShadowAtlasWidth;
            float invHalfShadowAtlasHeight = 0.5f * invShadowAtlasHeight;
            float softShadowsProp = ShadowUtils.SoftShadowQualityToShaderProperty(light, softShadows);

            float maxShadowDistanceSq = shadowData.mainLightShadowDistance * shadowData.mainLightShadowDistance;
            ShadowUtils.GetScaleAndBiasForLinearDistanceFade(maxShadowDistanceSq, shadowData.mainLightShadowCascadeBorder, out float shadowFadeScale, out float shadowFadeBias);

            cmd.SetGlobalMatrixArray(ShaderPropertyId.mainLightWorldToShadow, m_MainLightShadowMatrices);
            cmd.SetGlobalVector(ShaderPropertyId.mainLightShadowParams,
                new Vector4(light.shadowStrength, softShadowsProp, shadowFadeScale, shadowFadeBias));

            if (cascadeCount > 1)
            {
                cmd.SetGlobalVector(ShaderPropertyId.mainLightCascadeShadowSplitSpheres0,
                    m_CascadeSplitDistances[0]);
                cmd.SetGlobalVector(ShaderPropertyId.mainLightCascadeShadowSplitSpheres1,
                    m_CascadeSplitDistances[1]);
                cmd.SetGlobalVector(ShaderPropertyId.mainLightCascadeShadowSplitSpheres2,
                    m_CascadeSplitDistances[2]);
                cmd.SetGlobalVector(ShaderPropertyId.mainLightCascadeShadowSplitSpheres3,
                    m_CascadeSplitDistances[3]);
                cmd.SetGlobalVector(ShaderPropertyId.mainLightCascadeShadowSplitSphereRadii, new Vector4(
                    m_CascadeSplitDistances[0].w * m_CascadeSplitDistances[0].w,
                    m_CascadeSplitDistances[1].w * m_CascadeSplitDistances[1].w,
                    m_CascadeSplitDistances[2].w * m_CascadeSplitDistances[2].w,
                    m_CascadeSplitDistances[3].w * m_CascadeSplitDistances[3].w));
            }

            // Inside shader soft shadows are controlled through global keyword.
            // If any additional light has soft shadows it will force soft shadows on main light too.
            // As it is not trivial finding out which additional light has soft shadows, we will pass main light properties if soft shadows are supported.
            // This workaround will be removed once we will support soft shadows per light.
            if (shadowData.supportsSoftShadows)
            {
                cmd.SetGlobalVector(ShaderPropertyId.mainLightShadowOffset0,
                    new Vector4(-invHalfShadowAtlasWidth, -invHalfShadowAtlasHeight,
                        invHalfShadowAtlasWidth, -invHalfShadowAtlasHeight));
                cmd.SetGlobalVector(ShaderPropertyId.mainLightShadowOffset1,
                    new Vector4(-invHalfShadowAtlasWidth, invHalfShadowAtlasHeight,
                        invHalfShadowAtlasWidth, invHalfShadowAtlasHeight));

                cmd.SetGlobalVector(ShaderPropertyId.mainLightShadowmapSize, new Vector4(invShadowAtlasWidth,
                    invShadowAtlasHeight,
                    renderTargetWidth, renderTargetHeight));
            }
        }
    }
}