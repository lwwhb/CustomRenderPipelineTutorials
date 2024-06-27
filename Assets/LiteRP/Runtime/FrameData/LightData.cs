
using Unity.Collections;
using UnityEngine.Rendering;

namespace LiteRP.FrameData
{
    public class LightData : ContextItem
    {
        /// <summary>
        /// Holds the main light index from the <c>VisibleLight</c> list returned by culling. If there's no main light in the scene, <c>mainLightIndex</c> is set to -1.
        /// The main light is the directional light assigned as Sun source in light settings or the brightest directional light.
        /// <seealso cref="CullingResults"/>
        /// </summary>
        public int mainLightIndex;

        /// <summary>
        /// The number of additional lights visible by the camera.
        /// </summary>
        public int additionalLightsCount;

        /// <summary>
        /// Maximum amount of lights that can be shaded per-object. This value only affects forward rendering.
        /// </summary>
        public int maxPerObjectAdditionalLightsCount;

        /// <summary>
        /// List of visible lights returned by culling.
        /// </summary>
        public NativeArray<VisibleLight> visibleLights;
        

        /// <summary>
        /// True if mixed lighting is supported.
        /// </summary>
        public bool supportsMixedLighting;

        /// <summary>
        /// True if box projection is enabled for reflection probes.
        /// </summary>
        public bool reflectionProbeBoxProjection;

        /// <summary>
        /// True if blending is enabled for reflection probes.
        /// </summary>
        public bool reflectionProbeBlending;
        
        public override void Reset()
        {
            mainLightIndex = -1;
            additionalLightsCount = 0;
            maxPerObjectAdditionalLightsCount = 0;
            visibleLights = default;
            supportsMixedLighting = false;
            reflectionProbeBoxProjection = false;
            reflectionProbeBlending = false;
        }
    }
}