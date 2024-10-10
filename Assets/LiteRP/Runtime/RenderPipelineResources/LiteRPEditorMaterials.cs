#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Categorization;

namespace LiteRP.RenderPipelineResources
{
    [Serializable]
    [SupportedOnRenderPipeline(typeof(LiteRPAsset))]
    [CategoryInfo(Name = "R: Editor Materials", Order = 1000), HideInInspector]
    class LiteRPEditorMaterials : IRenderPipelineResources
    {
        public int version => 0;
        [SerializeField]
        [ResourcePath("Materials/Lit.mat")]
        private Material m_DefaultMaterial;
        public virtual Material defaultMaterial
        {
            get => m_DefaultMaterial;
            set => this.SetValueAndNotify(ref m_DefaultMaterial, value);
        }
    }
}
#endif

