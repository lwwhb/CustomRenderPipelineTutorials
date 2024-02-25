using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    public class LiteRenderPipeline : RenderPipeline
    {
        private static readonly ShaderTagId s_ShaderTagId = new ShaderTagId("SRPDefaultUnlit");
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
            
            //结束渲染上下文
            EndContextRendering(context, cameras);
        }

        private void RenderCamera(ScriptableRenderContext context, Camera camera)
        {
            //开始渲染相机
            BeginCameraRendering(context, camera);
            //获取相机剔除参数，并进行剔除
            ScriptableCullingParameters cullingParameters;
            if (!camera.TryGetCullingParameters(out cullingParameters))
                return;
            CullingResults cullingResults = context.Cull(ref cullingParameters);
            //为相机创建CommandBuffer
            CommandBuffer cmd = CommandBufferPool.Get(camera.name);
            //设置相机属性参数
            context.SetupCameraProperties(camera);

            var clearFlags = camera.clearFlags;
            bool clearSkybox = clearFlags == CameraClearFlags.Skybox;
            bool clearDepth = clearFlags != CameraClearFlags.Nothing;
            bool clearColor = clearFlags == CameraClearFlags.Color;
            //清理渲染目标
            cmd.ClearRenderTarget(clearDepth, clearColor, CoreUtils.ConvertSRGBToActiveColorSpace(camera.backgroundColor));
            
            if (clearSkybox)
            {
                //绘制天空盒
                var skyboxRendererList = context.CreateSkyboxRendererList(camera);
                cmd.DrawRendererList(skyboxRendererList);
            }

            //指定渲染排序设置SortSettings
            var sortSettings = new SortingSettings(camera);
            //指定渲染状态设置DrawSettings
            var drawSettings = new DrawingSettings(s_ShaderTagId, sortSettings);
            
            //绘制不透明物体
            sortSettings.criteria = SortingCriteria.CommonOpaque;
            //指定渲染过滤设置FilterSettings
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
            //创建渲染列表
            var rendererListParams = new RendererListParams(cullingResults, drawSettings, filterSettings);
            var rendererList = context.CreateRendererList(ref rendererListParams);
            //绘制渲染列表
            cmd.DrawRendererList(rendererList);
            
            //绘制半透明物体
            sortSettings.criteria = SortingCriteria.CommonTransparent;
            //指定渲染过滤设置FilterSettings
            filterSettings = new FilteringSettings(RenderQueueRange.transparent);
            //创建渲染列表
            rendererListParams = new RendererListParams(cullingResults, drawSettings, filterSettings);
            rendererList = context.CreateRendererList(ref rendererListParams);
            //绘制渲染列表
            cmd.DrawRendererList(rendererList);
            
            
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
    }
}