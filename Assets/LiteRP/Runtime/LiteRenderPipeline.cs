using System.Collections.Generic;
using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public class LiteRenderPipeline : RenderPipeline
    {
        private static readonly ShaderTagId s_shaderTagId = new ShaderTagId("SRPDefaultUnlit"); 
        
        private RenderGraph m_RenderGraph = null;									//使用的RenderGraph
        private LiteRenderGraphRecorder m_LiteRenderGraphRecorder = null;		    //使用的LiteRenderGraphRecorder
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
                
                //准备FrameData
                if(!PrepareFrameData(context, camera))
                    continue;
                
                CommandBuffer cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, new ProfilingSampler(camera.name)))
                {
                    //记录并执行RenderGraph
                    RecordAndExecuteRenderGraph(context, camera, cmd);
                }
                //提交命令缓冲区
                context.ExecuteCommandBuffer(cmd);
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
            m_RenderGraph.nativeRenderPassesEnabled = LiteRPUtils.IsNativeRenderPassesEnabled();
            m_LiteRenderGraphRecorder = new LiteRenderGraphRecorder();
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
            m_LiteRenderGraphRecorder.RecordRenderGraph(m_RenderGraph, m_ContextData);
            m_RenderGraph.EndRecordingAndExecute();
        }

        private bool PrepareFrameData(ScriptableRenderContext context, Camera camera)
        {
            RenderData renderData = m_ContextData.GetOrCreate<RenderData>();
            renderData.renderContext = context;
            
            //Culling
            ScriptableCullingParameters cullingParams;
            if (!camera.TryGetCullingParameters(out cullingParams))
                return false;
            
            CameraData cameraData = m_ContextData.GetOrCreate<CameraData>();
            cameraData.camera = camera;
            cameraData.cullingResults = context.Cull(ref cullingParams);

            return true;
        }

        private void CleanupRenderGraph()
        {
            if (m_ContextData != null)
            {
                m_ContextData.Dispose();
                m_ContextData = null;
            }
            
            if (m_LiteRenderGraphRecorder != null)
            {
                m_LiteRenderGraphRecorder.Dispose();
                m_LiteRenderGraphRecorder = null;
            }
            
            if (m_RenderGraph != null)
            {
                m_RenderGraph.Cleanup();
                m_RenderGraph = null;
            }
        }
    }
}