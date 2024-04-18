
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    [CreateAssetMenu(menuName = "Lite Render Pipeline/Lite Render Pipeline Asset")]
    public class LiteRPAsset : RenderPipelineAsset<LiteRenderPipeline>
    {
        [SerializeField] bool m_UseSRPBatcher = true;
        public bool useSRPBatcher
        {
            get => m_UseSRPBatcher;
            set => m_UseSRPBatcher = value;
        }
       
        [SerializeField] GPUResidentDrawerMode m_GPUResidentDrawerMode = GPUResidentDrawerMode.Disabled;
        public GPUResidentDrawerMode gpuResidentDrawerMode
        {
            get => m_GPUResidentDrawerMode;
            set
            {
                if (value == m_GPUResidentDrawerMode)
                    return;

                m_GPUResidentDrawerMode = value;
                OnValidate();
            }
        }
        
        [SerializeField] float m_SmallMeshScreenPercentage = 0.0f;
        public float smallMeshScreenPercentage
        {
            get => m_SmallMeshScreenPercentage;
            set
            {
                if (Math.Abs(value - m_SmallMeshScreenPercentage) < float.Epsilon)
                    return;

                m_SmallMeshScreenPercentage = Mathf.Clamp(value, 0.0f, 20.0f);
                OnValidate();
            }
        }

        [SerializeField] bool m_GPUResidentDrawerEnableOcclusionCullingInCameras;
        public bool gpuResidentDrawerEnableOcclusionCullingInCameras
        {
            get => m_GPUResidentDrawerEnableOcclusionCullingInCameras;
            set
            {
                if (value == m_GPUResidentDrawerEnableOcclusionCullingInCameras)
                    return;

                m_GPUResidentDrawerEnableOcclusionCullingInCameras = value;
                OnValidate();
            }
        }
        
        [SerializeField] int m_AntiAliasing = 1;
        public int antiAliasing
        {
            get => m_AntiAliasing;
            set => m_AntiAliasing = value;
        }
        protected override RenderPipeline CreatePipeline()
        {
            return new LiteRenderPipeline(this);
        }
    }
}
