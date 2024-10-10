#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Categorization;

namespace LiteRP.RenderPipelineResources
{
    [Serializable]
    [SupportedOnRenderPipeline(typeof(LiteRPAsset))]
    [CategoryInfo(Name = "R: Editor Shaders", Order = 1000), HideInInspector]
    class LiteRPEditorShaders : IRenderPipelineResources
    {
        public int version => 0;
    }
}
#endif

