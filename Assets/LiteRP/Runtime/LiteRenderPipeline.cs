using System;
using System.Collections.Generic;
using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public class LiteRenderPipeline : RenderPipeline
    {
        private LiteRPAsset m_Asset;
        private RenderGraph m_RenderGraph = null; //渲染图
        private LiteRPRenderGraphRecorder m_LiteRPRenderGraphRecorder = null; //渲染图记录器
        private ContextContainer m_ContextContainer = null; //上下文容器

        public LiteRenderPipeline(LiteRPAsset asset)
        {
            m_Asset = asset;
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
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            //不实现
        }
        
        //新版本
        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
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
            bool cameraRenderShadows = true;//cameraData.maxShadowDistance > 0.0f;      //注意条件
            shadowData.mainLightShadowEnabled = m_Asset.mainLightShadowEnabled;
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
                shadowData.mainLightShadowBias = new Vector4(m_Asset.mainLightShadowDepthBias, m_Asset.mainLightShadowNormalBias, 0.0f, 0.0f);  //临时写法
                shadowData.mainLightShadowmapResolution = (int)light.shadowResolution;
            }
            shadowData.supportsSoftShadows = m_Asset.supportsSoftShadows && shadowData.supportMainLightShadow;
            
;           return true;
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
    }
}