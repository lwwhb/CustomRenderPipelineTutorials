using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    public class LiteRenderPipeline : RenderPipeline
    {
        private static readonly ShaderTagId s_shaderTagId = new ShaderTagId("SRPDefaultUnlit"); 
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
                
                var backgroundColorSRGB = camera.backgroundColor;
                cmd.ClearRenderTarget(true, true, CoreUtils.ConvertSRGBToActiveColorSpace(backgroundColorSRGB));
                
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
                cmd.DrawRendererList(rl);
                
                /*RenderGraphParameters renderGraphParams = new RenderGraphParameters()
                {
                    executionName = camera.name,
                    commandBuffer = cmd,
                    scriptableRenderContext = context,
                    currentFrameIndex = Time.frameCount,
                };
                m_renderGraph.BeginRecording(renderGraphParams);

                
                m_renderGraph.EndRecordingAndExecute();*/
                
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
            //结束上下文渲染
            EndContextRendering(context, cameras);
        }
    }
}