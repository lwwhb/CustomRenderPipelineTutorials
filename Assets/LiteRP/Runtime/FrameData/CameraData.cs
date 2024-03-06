using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.FrameData
{
    public class CameraData : ContextItem
    {
        public Camera camera;
        public CullingResults cullingResults;

        public override void Reset()
        {
            camera = null;
            cullingResults = default;
        }
    }
}