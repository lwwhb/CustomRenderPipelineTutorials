using System;
using System.Collections.Generic;
using LiteRP.AdditionalData;
using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public class LiteRenderPipeline : RenderPipeline
    {
        public const string k_ShaderTagName = "LiteRenderPipeline";
        
        private LiteRPAsset m_Asset = null;
        private LiteRPGlobalSettings m_GlobalSettings = null;  //管线全局设置
        private RenderGraph m_RenderGraph = null; //渲染图
        private LiteRPRenderGraphRecorder m_LiteRPRenderGraphRecorder = null; //渲染图记录器
        private ContextContainer m_ContextContainer = null; //上下文容器

        public static LiteRPAsset asset
        {
            get => GraphicsSettings.currentRenderPipeline as LiteRPAsset;
        }
        public override RenderPipelineGlobalSettings defaultSettings => m_GlobalSettings;

        public LiteRenderPipeline(LiteRPAsset asset)
        {
            m_Asset = asset;
            // 设置管线的全局设置
            m_GlobalSettings = LiteRPGlobalSettings.instance;
            // 初始化引擎渲染功能
            InitSupportedRenderingFeatures(asset);
            // 初始化管线属性
            InitializeRPSettings();
            // 初始化RTHandle System
            RTHandles.Initialize(Screen.width, Screen.height);
            // 初始化全局Shader关键字
            ShaderGlobalKeywords.InitializeShaderGlobalKeywords();
            // 初始化RenderGraph
            InitializeRenderGraph();
        }
        protected override void Dispose(bool disposing)
        {
            CleanupRenderGraph();
            ResetSupportedRenderingFeatures();
            base.Dispose(disposing);
        }
        //初始化Settings
        private void InitializeRPSettings()
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = m_Asset.useSRPBatcher;   
            QualitySettings.antiAliasing = m_Asset.antiAliasing;
        }
        //初始化渲染图
        private void InitializeRenderGraph()
        {
            m_RenderGraph = new RenderGraph("LiteRPRenderGraph");
            m_RenderGraph.nativeRenderPassesEnabled = LiteRPRenderGraphUtils.IsSupportsNativeRenderPassRenderGraphCompiler();
            m_LiteRPRenderGraphRecorder = new LiteRPRenderGraphRecorder();
            m_ContextContainer = new ContextContainer();
        }
        //清理渲染图
        private void CleanupRenderGraph()
        {
            m_ContextContainer?.Dispose();
            m_ContextContainer = null;
            m_LiteRPRenderGraphRecorder?.Dispose();
            m_LiteRPRenderGraphRecorder = null;
            m_RenderGraph?.Cleanup();
            m_RenderGraph = null;
        }

        //老版本
        /*protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            //不实现
        }*/
        
        //新版本
        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            //检查管线全局渲染设置
            CheckGlobalRenderingSettings();
            
            //开始渲染上下文
            BeginContextRendering(context, cameras);
            
            //遍历渲染相机
            for (int i = 0; i < cameras.Count; i++)
            {
                Camera camera = cameras[i];
                RenderCamera(context, camera);
            }
            //结束渲染图
            m_RenderGraph.EndFrame();
            //结束渲染上下文
            EndContextRendering(context, cameras);
        }

        private void RenderCamera(ScriptableRenderContext context, Camera camera)
        {
            //开始渲染相机
            BeginCameraRendering(context, camera);
            //准备FrameData
            if(!PrepareFrameData(context, camera))
                return;
            //为相机创建CommandBuffer
            CommandBuffer cmd = CommandBufferPool.Get();
            //设置每个相机的Shader环境光参数
            SetupPerCameraShaderConstants(cmd);
            //记录并执行渲染图
            RecordAndExecuteRenderGraph(context, camera, cmd);
            //提交命令缓冲区
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            //提交渲染上下文
            context.Submit();
            //结束渲染相机
            EndCameraRendering(context, camera);
        }

        private void CheckGlobalRenderingSettings()
        {
            GraphicsSettings.lightsUseLinearIntensity = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            GraphicsSettings.lightsUseColorTemperature = true;
        }

        private void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, Camera camera)
        {
            float maxShadowDistance = Mathf.Min(m_Asset.mainLightShadowDistance, camera.farClipPlane);
            bool isShadowCastingDisabled = !m_Asset.mainLightShadowEnabled;
            bool isShadowDistanceZero = Mathf.Approximately(maxShadowDistance, 0.0f);
            if (isShadowCastingDisabled || isShadowDistanceZero)
            {
                cullingParameters.cullingOptions &= ~CullingOptions.ShadowCasters;
            }
            cullingParameters.maximumVisibleLights = 1;     //只有主光源
            cullingParameters.shadowDistance = maxShadowDistance;

            //设置保守剔除
            cullingParameters.conservativeEnclosingSphere = m_Asset.conservativeEnclosingSphere;
            cullingParameters.numIterationsEnclosingSphere = m_Asset.numIterationsEnclosingSphere;
        }

        private bool PrepareFrameData(ScriptableRenderContext context, Camera camera)
        {
            //获取相机剔除参数，并进行剔除
            if (!camera.TryGetCullingParameters(out var cullingParameters))
                return false;
            //构建剔除参数
            SetupCullingParameters(ref cullingParameters, camera);
            CullingResults cullingResults = context.Cull(ref cullingParameters);
            
            // 初始化摄像机帧数据
            CameraData cameraData = m_ContextContainer.GetOrCreate<CameraData>();
            cameraData.camera = camera;
            cameraData.cullingResults = cullingResults;
            bool anyShadowsEnabled = m_Asset.mainLightShadowEnabled;
            cameraData.maxShadowDistance = Mathf.Min(m_Asset.mainLightShadowDistance, camera.farClipPlane);
            cameraData.maxShadowDistance = (anyShadowsEnabled && cameraData.maxShadowDistance >= camera.nearClipPlane) ? cameraData.maxShadowDistance : 0.0f;
            // 初始化摄像机附加管线数据
            AdditionalCameraData additionalCameraData = null;
            camera.gameObject.TryGetComponent(out additionalCameraData);
            if (additionalCameraData != null)
            {
                cameraData.postProcessEnabled = additionalCameraData.renderPostProcessing;
                cameraData.maxShadowDistance = additionalCameraData.renderShadows ? cameraData.maxShadowDistance : 0.0f;
            }
            
            //初始化灯光帧数据
            LightData lightData = m_ContextContainer.GetOrCreate<LightData>();
            var visibleLights = cullingResults.visibleLights;
            lightData.mainLightIndex = LightUtils.GetMainLightIndex(visibleLights);
            lightData.additionalLightsCount = Math.Min((lightData.mainLightIndex != -1) ? visibleLights.Length - 1 : visibleLights.Length, LightUtils.maxVisibleAdditionalLights);
            lightData.maxPerObjectAdditionalLightsCount = Math.Min(m_Asset.maxAdditionalLightsCount, LiteRPAsset.k_MaxPerObjectLights);
            lightData.visibleLights = visibleLights;
            
            //初始化阴影帧数据
            ShadowData shadowData = m_ContextContainer.GetOrCreate<ShadowData>();
            // maxShadowDistance is set to 0.0f when the Render Shadows toggle is disabled on the camera
            bool cameraRenderShadows = cameraData.maxShadowDistance > 0.0f;      
            shadowData.mainLightShadowEnabled = anyShadowsEnabled;
            shadowData.supportMainLightShadow = SystemInfo.supportsShadows && shadowData.mainLightShadowEnabled && cameraRenderShadows;
            shadowData.mainLightShadowDistance = cullingParameters.shadowDistance;
            
            shadowData.shadowmapDepthBufferBits = 16;
            shadowData.mainLightShadowCascadeBorder = m_Asset.mainLightCascadeBorder;
            shadowData.mainLightShadowCascadesCount = m_Asset.mainLightShadowCascadesCount;
            shadowData.mainLightShadowCascadesSplit = ShadowUtils.GetMainLightCascadeSplit(shadowData.mainLightShadowCascadesCount, m_Asset);
            shadowData.mainLightShadowmapWidth = m_Asset.mainLightShadowmapResolution;
            shadowData.mainLightShadowmapHeight = m_Asset.mainLightShadowmapResolution;
            ShadowUtils.CreateShadowAtlasAndCullShadowCasters(shadowData, ref cameraData.cullingResults, ref context);
            
            var mainLightIndex = lightData.mainLightIndex;
            if (mainLightIndex < 0)     //注意这里小于0的情况
                return true;
            
            VisibleLight vl = visibleLights[mainLightIndex];
            Light light = vl.light;
            shadowData.supportMainLightShadow &= mainLightIndex != -1
                                                 && light != null
                                                 && light.shadows != LightShadows.None;
            
            if (!shadowData.supportMainLightShadow)
            {
                shadowData.mainLightShadowBias = Vector4.zero;
                shadowData.mainLightShadowmapResolution = 0;
            }
            else
            {
                // 初始化灯光附加管线数据
                AdditionalLightData data = null;
                if (light != null)
                    light.gameObject.TryGetComponent(out data);
                if (data && !data.usePipelineSettings)
                    shadowData.mainLightShadowBias = new Vector4(light.shadowBias, light.shadowNormalBias, 0.0f, 0.0f);  
                else
                    shadowData.mainLightShadowBias = new Vector4(m_Asset.mainLightShadowDepthBias, m_Asset.mainLightShadowNormalBias, 0.0f, 0.0f);
                shadowData.mainLightShadowmapResolution = m_Asset.mainLightShadowmapResolution;
            }
            shadowData.supportsSoftShadows = m_Asset.supportsSoftShadows && shadowData.supportMainLightShadow;
            
;           return true;
        }
        
        private void SetupPerCameraShaderConstants(CommandBuffer cmd)
        {
            // When glossy reflections are OFF in the shader we set a constant color to use as indirect specular
            SphericalHarmonicsL2 ambientSH = RenderSettings.ambientProbe;
            Color linearGlossyEnvColor = new Color(ambientSH[0, 0], ambientSH[1, 0], ambientSH[2, 0]) * RenderSettings.reflectionIntensity;
            Color glossyEnvColor = CoreUtils.ConvertLinearToActiveColorSpace(linearGlossyEnvColor);
            cmd.SetGlobalVector(ShaderPropertyId.glossyEnvironmentColor, glossyEnvColor);
            
            Vector4 unity_SHAr = new Vector4(ambientSH[0, 3], ambientSH[0, 1], ambientSH[0, 2], ambientSH[0, 0] - ambientSH[0, 6]);
            Vector4 unity_SHAg = new Vector4(ambientSH[1, 3], ambientSH[1, 1], ambientSH[1, 2], ambientSH[1, 0] - ambientSH[1, 6]);
            Vector4 unity_SHAb = new Vector4(ambientSH[2, 3], ambientSH[2, 1], ambientSH[2, 2], ambientSH[2, 0] - ambientSH[2, 6]);
            
            Vector4 unity_SHBr = new Vector4(ambientSH[0, 4], ambientSH[0, 6], ambientSH[0, 5] * 3, ambientSH[0, 7]);
            Vector4 unity_SHBg = new Vector4(ambientSH[1, 4], ambientSH[1, 6], ambientSH[1, 5] * 3, ambientSH[1, 7]);
            Vector4 unity_SHBb = new Vector4(ambientSH[2, 4], ambientSH[2, 6], ambientSH[2, 5] * 3, ambientSH[2, 7]);
            
            Vector4 unity_SHC = new Vector4(ambientSH[0, 8], ambientSH[2, 8], ambientSH[1, 8], 1);
            
            cmd.SetGlobalVector(ShaderPropertyId.shAr, unity_SHAr);
            cmd.SetGlobalVector(ShaderPropertyId.shAg, unity_SHAg);
            cmd.SetGlobalVector(ShaderPropertyId.shAb, unity_SHAb);
            cmd.SetGlobalVector(ShaderPropertyId.shBr, unity_SHBr);
            cmd.SetGlobalVector(ShaderPropertyId.shBg, unity_SHBg);
            cmd.SetGlobalVector(ShaderPropertyId.shBb, unity_SHBb);
            cmd.SetGlobalVector(ShaderPropertyId.shC, unity_SHC);
            
            // Used as fallback cubemap for reflections
            cmd.SetGlobalTexture(ShaderPropertyId.glossyEnvironmentCubeMap, ReflectionProbe.defaultTexture);
            cmd.SetGlobalVector(ShaderPropertyId.glossyEnvironmentCubeMapHDR, ReflectionProbe.defaultTextureHDRDecodeValues);

            // Ambient
            cmd.SetGlobalVector(ShaderPropertyId.ambientSkyColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientSkyColor));
            cmd.SetGlobalVector(ShaderPropertyId.ambientEquatorColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientEquatorColor));
            cmd.SetGlobalVector(ShaderPropertyId.ambientGroundColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientGroundColor));
        }

        private void RecordAndExecuteRenderGraph(ScriptableRenderContext context, Camera camera, CommandBuffer cmd)
        {
            RenderGraphParameters renderGraphParameters = new RenderGraphParameters()
            {
                executionName = camera.name,
                commandBuffer = cmd,
                scriptableRenderContext = context,
                currentFrameIndex = Time.frameCount
            };
            m_RenderGraph.BeginRecording(renderGraphParameters);
            //开启录制时间线
            m_LiteRPRenderGraphRecorder.RecordRenderGraph(m_RenderGraph, m_ContextContainer);
            m_RenderGraph.EndRecordingAndExecute();
        }
        
        static void InitSupportedRenderingFeatures(LiteRPAsset pipelineAsset)
        {
#if UNITY_EDITOR
            SupportedRenderingFeatures.active = new SupportedRenderingFeatures()
            {
                ambientProbeBaking = true,
                defaultMixedLightingModes = SupportedRenderingFeatures.LightmapMixedBakeModes.Subtractive,
                defaultReflectionProbeBaking = true,
                editableMaterialRenderQueue = true,
                enlighten = true,
                lightmapBakeTypes = LightmapBakeType.Baked | LightmapBakeType.Mixed | LightmapBakeType.Realtime,
                lightmapsModes = LightmapsMode.CombinedDirectional | LightmapsMode.NonDirectional,
                lightProbeProxyVolumes = false,
                mixedLightingModes = SupportedRenderingFeatures.LightmapMixedBakeModes.Subtractive | SupportedRenderingFeatures.LightmapMixedBakeModes.IndirectOnly | SupportedRenderingFeatures.LightmapMixedBakeModes.Shadowmask,
                motionVectors = false,
                overridesEnableLODCrossFade = true,
                overridesEnvironmentLighting = false,
                overridesFog = false,
                overridesLightProbeSystem = false,
                overridesLODBias = false,
                overridesMaximumLODLevel = false,
                overridesOtherLightingSettings = false,
                overridesRealtimeReflectionProbes = false,
                overridesShadowmask = false,
                particleSystemInstancing = true,
                receiveShadows = false,
                reflectionProbeModes = SupportedRenderingFeatures.ReflectionProbeModes.None,
                reflectionProbes = false,
                reflectionProbesBlendDistance = false,
                rendererPriority = false,
                rendererProbes = true,
                rendersUIOverlay = false,
                skyOcclusion = false,
                supportsClouds = false,
                supportsHDR = false
            };
#endif

            SupportedRenderingFeatures.active.supportsHDR = pipelineAsset.supportsHDR;
            SupportedRenderingFeatures.active.rendersUIOverlay = false;
        }

        static void ResetSupportedRenderingFeatures()
        {
            SupportedRenderingFeatures.active = new SupportedRenderingFeatures();
        }
    }
}