using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public class LiteRenderPipeline : RenderPipeline
    {
        private static readonly ShaderTagId s_shaderTagId = new ShaderTagId("SRPDefaultUnlit"); 
        
        private RenderGraph m_RenderGraph = null;									//使用的RenderGraph
        private LiteRenderer m_LiteRenderer = null;								    //使用的LiteRenderer
        private ContextContainer m_ContextData = null;								//上下文数据容器
       
        
        public LiteRenderPipeline()
        {
            RTHandles.Initialize(Screen.width, Screen.height);
            InitializeRenderGraph();
        }

        protected override void Dispose(bool disposing)
        {
            CleanupRenderGraph();
            base.Dispose(disposing);
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            Render(context, new List<Camera>(cameras));
        }
        // 渲染实现
        protected override void Render(ScriptableRenderContext context, List<Camera> cameras) 
        {
            //开始上下文渲染
            BeginContextRendering(context, cameras);
            //迭代处理所有相机
            for (int i = 0; i < cameras.Count; ++i)
            {
                var camera = cameras[i];
                //开始相机渲染
                BeginCameraRendering(context, camera);
                
                //Culling
                ScriptableCullingParameters cullingParams;
                if (!camera.TryGetCullingParameters(out cullingParams))
                    continue;
                CullingResults cull = context.Cull(ref cullingParams);
                
                CommandBuffer cmd = CommandBufferPool.Get(camera.name);
                //设置相机参数
                cmd.SetupCameraProperties(camera);
                
                //记录并执行RenderGraph
                RecordAndExecuteRenderGraph(context, camera, cmd);
                
                /*var clearFlags = camera.clearFlags;
                bool drawSkyBox = clearFlags == CameraClearFlags.Skybox? true : false;
                bool clearDepth = clearFlags != CameraClearFlags.Nothing;
                bool clearColor = clearFlags == CameraClearFlags.Color? true : false;
                
                var backgroundColorSRGB = camera.backgroundColor;
                cmd.ClearRenderTarget(clearDepth, clearColor, CoreUtils.ConvertSRGBToActiveColorSpace(backgroundColorSRGB));

                if (drawSkyBox)
                {
                    RendererList skyboxrl = context.CreateSkyboxRendererList(camera);
                    cmd.DrawRendererList(skyboxrl);
                }
                
                //Setup DrawSettings and FilterSettings
                var sortingSettings = new SortingSettings(camera);
                DrawingSettings drawSettings = new DrawingSettings(s_shaderTagId, sortingSettings);
                FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all);
                
                //Opaque objects
                sortingSettings.criteria = SortingCriteria.CommonOpaque;
                drawSettings.sortingSettings = sortingSettings;
                filterSettings.renderQueueRange = RenderQueueRange.opaque;
                
                RendererListParams rlp = new RendererListParams(cull, drawSettings, filterSettings);
                RendererList rl = context.CreateRendererList(ref rlp);
                cmd.name = "Render Opaque Objects";
                cmd.DrawRendererList(rl);
                
                //Transparent objects
                sortingSettings.criteria = SortingCriteria.CommonTransparent;
                drawSettings.sortingSettings = sortingSettings;
                filterSettings.renderQueueRange = RenderQueueRange.transparent;
                rlp = new RendererListParams(cull, drawSettings, filterSettings);
                rl = context.CreateRendererList(ref rlp);
                cmd.name = "Render Transparent Objects";
                cmd.DrawRendererList(rl);*/
                
                
                //提交命令缓冲区
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                //释放命令缓冲区
                CommandBufferPool.Release(cmd);
                //提交上下文
                context.Submit();
                //结束相机渲染
                EndCameraRendering(context, camera);
            }
            //RenderGraph结束帧
            m_RenderGraph.EndFrame();
            //结束上下文渲染
            EndContextRendering(context, cameras);
        }

        private void InitializeRenderGraph()
        {
            m_RenderGraph = new RenderGraph("LiteRPRenderGraph");
            m_LiteRenderer = new LiteRenderer();
            m_ContextData = new ContextContainer();
        }
        private void RecordAndExecuteRenderGraph(ScriptableRenderContext context, Camera camera, CommandBuffer cmd)
        {
            RenderGraphParameters renderGraphParams = new RenderGraphParameters()
            {
                executionName = camera.name,
                commandBuffer = cmd,
                scriptableRenderContext = context,
                currentFrameIndex = Time.frameCount,
            };
            m_RenderGraph.BeginRecording(renderGraphParams);
            m_LiteRenderer.RecordRenderGraph(m_RenderGraph, context, camera);
            m_RenderGraph.EndRecordingAndExecute();
        }

        private void CleanupRenderGraph()
        {
            if (m_ContextData != null)
            {
                m_ContextData.Dispose();
                m_ContextData = null;
            }
            
            if (m_LiteRenderer != null)
            {
                m_LiteRenderer.Dispose();
                m_LiteRenderer = null;
            }
            
            if (m_RenderGraph != null)
            {
                m_RenderGraph.Cleanup();
                m_RenderGraph = null;
            }
        }
    }
}