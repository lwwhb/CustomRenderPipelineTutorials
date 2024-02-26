using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public class LiteRenderPipeline : RenderPipeline
    {
        private static readonly ShaderTagId s_ShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        
        private RenderGraph m_RenderGraph = null; //渲染图
        private LiteRenderGraphRecorder m_LiteRenderGraphRecorder = null; //渲染图记录器
        private ContextContainer m_ContextContainer = null; //上下文容器

        public LiteRenderPipeline()
        {
            InitializeRenderGraph();
        }
        protected override void Dispose(bool disposing)
        {
            CleanupRenderGraph();
            base.Dispose(disposing);
        }
        //初始化渲染图
        private void InitializeRenderGraph()
        {
            m_RenderGraph = new RenderGraph("LiteRPRenderGraph");
            m_LiteRenderGraphRecorder = new LiteRenderGraphRecorder();
            m_ContextContainer = new ContextContainer();
        }
        //清理渲染图
        private void CleanupRenderGraph()
        {
            m_ContextContainer?.Dispose();
            m_ContextContainer = null;
            m_LiteRenderGraphRecorder = null;
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
            //获取相机剔除参数，并进行剔除
            ScriptableCullingParameters cullingParameters;
            if (!camera.TryGetCullingParameters(out cullingParameters))
                return;
            CullingResults cullingResults = context.Cull(ref cullingParameters);
            //为相机创建CommandBuffer
            CommandBuffer cmd = CommandBufferPool.Get(camera.name);
            //设置相机属性参数
            context.SetupCameraProperties(camera);
            //记录并执行渲染图
            RecordAndExecuteRenderGraph(context, camera, cmd);
            //提交命令缓冲区
            context.ExecuteCommandBuffer(cmd);
            //释放命令缓冲区
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            //提交渲染上下文
            context.Submit();
            //结束渲染相机
            EndCameraRendering(context, camera);
        }

        private bool PrepareFrameData(ScriptableRenderContext context, Camera camera)
        {
            //下节课实现
            return true;
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
            m_LiteRenderGraphRecorder.RecordRenderGraph(m_RenderGraph, m_ContextContainer);
            m_RenderGraph.EndRecordingAndExecute();
        }
    }
}