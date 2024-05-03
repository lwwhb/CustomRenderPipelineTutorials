using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.FrameData
{
    public class ShadowData : ContextItem
    {
        // 是否开启主光源阴影
        public bool mainLightShadowEnabled;
        
        // 是否支持主光源阴影
        public bool supportMainLightShadow;

        // 主光源阴影贴图宽度
        public int mainLightShadowmapWidth;
        
        // 主光源阴影贴图高度
        public int mainLightShadowmapHeight;
        
        // 主光源阴影显示范围
        public float mainLightShadowDistance;
        
        // Cascades ShadowMap 级数
        public int mainLightShadowCascadesCount;
        
        // 级数划分
        public Vector3 mainLightShadowCascadesSplit;
        
        // 分级边缘过渡 0-1
        public float mainLightShadowCascadeBorder;
        
        //是否支持软阴影
        public bool supportsSoftShadows;

        // ShadowMap深度位数
        public int shadowmapDepthBufferBits;

        // 主光源 shadow bias
        public Vector4 mainLightShadowBias;
        
        // 主光源 ShadowMap 分辨率
        public int mainLightShadowmapResolution;
        
        internal int mainLightShadowResolution;
        internal int mainLightRenderTargetWidth;
        internal int mainLightRenderTargetHeight;

        internal NativeArray<LightShadowCullingInfos> visibleLightsShadowCullingInfos;
        
        public override void Reset()
        {
            mainLightShadowEnabled = false;
            supportMainLightShadow = false;
            mainLightShadowmapWidth = 0;
            mainLightShadowmapHeight = 0;
            mainLightShadowDistance = 0;
            mainLightShadowCascadesCount = 0;
            mainLightShadowCascadesSplit = Vector3.zero;
            mainLightShadowCascadeBorder = 0;
            supportsSoftShadows = false;
            shadowmapDepthBufferBits = 0;
            mainLightShadowBias = Vector4.zero;
            mainLightShadowmapResolution = 0;
            
            mainLightShadowResolution = 0;
            mainLightRenderTargetWidth = 0;
            mainLightRenderTargetHeight = 0;

            visibleLightsShadowCullingInfos = default;
        }
    }
}