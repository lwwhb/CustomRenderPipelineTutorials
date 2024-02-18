using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    [CreateAssetMenu(menuName = "Lite Render Pipeline/Lite Render Pipeline Asset")]
    public class LiteRPAsset : RenderPipelineAsset<LiteRenderPipeline>
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new LiteRenderPipeline();
        }
    }
}
