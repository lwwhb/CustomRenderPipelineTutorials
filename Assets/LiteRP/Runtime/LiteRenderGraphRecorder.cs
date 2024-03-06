using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder : IRenderGraphRecorder
    {
        public void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            //下节课实现
            AddDrawObjectsPass(renderGraph, frameData);
        }
    }
}