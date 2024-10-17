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
        
        [SerializeField]
        [ResourcePath("Materials/ParticlesUnlit.mat")]
        private Material m_DefaultParticleMaterial;

        public virtual Material defaultParticleUnlitMaterial
        {
            get => m_DefaultParticleMaterial;
            set => this.SetValueAndNotify(ref m_DefaultParticleMaterial, value);
        }

        [SerializeField]
        [ResourcePath("Materials/ParticlesUnlit.mat")]
        private Material m_DefaultLineMaterial;

        public virtual Material defaultLineMaterial
        {
            get => m_DefaultLineMaterial;
            set => this.SetValueAndNotify(ref m_DefaultLineMaterial, value);
        }
    }
}
#endif

