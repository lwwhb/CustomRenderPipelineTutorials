
using UnityEngine.Rendering;

namespace LiteRP.FrameData
{
    public class RenderData : ContextItem
    {
        public ScriptableRenderContext renderContext;
        public override void Reset()
        {
            renderContext = default;
        }
    }
}