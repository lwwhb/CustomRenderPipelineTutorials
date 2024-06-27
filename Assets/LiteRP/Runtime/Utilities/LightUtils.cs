
using LiteRP.FrameData;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    public static class LightUtils
    {
        // Holds light direction for directional lights or position for punctual lights.
        // When w is set to 1.0, it means it's a punctual light.
        internal static Vector4 k_DefaultLightPosition = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
        internal static Vector4 k_DefaultLightColor = Color.black;

        // Default light attenuation is setup in a particular way that it causes
        // directional lights to return 1.0 for both distance and angle attenuation
        internal static Vector4 k_DefaultLightAttenuation = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        internal static Vector4 k_DefaultLightSpotDirection = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
        
        //根据平台确定最大可见光源数量
        public static int maxVisibleAdditionalLights
        {
            get
            {
                // Must match: Input.hlsl, MAX_VISIBLE_LIGHTS
                bool isMobileOrMobileBuildTarget = GraphicsSettings.HasShaderDefine(BuiltinShaderDefine.SHADER_API_MOBILE);
                if (isMobileOrMobileBuildTarget && (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && Graphics.minOpenGLESVersion <= OpenGLESVersion.OpenGLES30))
                    return ShaderOptions.k_MaxVisibleLightCountLowEndMobile;

                // GLES can be selected as platform on Windows (not a mobile platform) but uniform buffer size so we must use a low light count.
                // WebGPU's minimal limits are based on mobile rather than desktop, so it will need to assume mobile.
                return (isMobileOrMobileBuildTarget || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.WebGPU)
                    ? ShaderOptions.k_MaxVisibleLightCountMobile : ShaderOptions.k_MaxVisibleLightCountDesktop;
            }
        }
        // 寻找主光源，优先SunLight，然后按亮度最大查找主光源
        internal static int GetMainLightIndex(NativeArray<VisibleLight> visibleLights)
        {
            int totalVisibleLights = visibleLights.Length;
            if (totalVisibleLights == 0)
                return -1;

            Light sunLight = RenderSettings.sun;
            int brightestDirectionalLightIndex = -1;
            float brightestLightIntensity = 0.0f;
            for (int i = 0; i < totalVisibleLights; ++i)
            {
                ref VisibleLight currVisibleLight = ref visibleLights.UnsafeElementAtMutable(i);
                Light currLight = currVisibleLight.light;

                // Particle system lights have the light property as null. We sort lights so all particles lights
                // come last. Therefore, if first light is particle light then all lights are particle lights.
                // In this case we either have no main light or already found it.
                if (currLight == null)
                    break;

                if (currVisibleLight.lightType == LightType.Directional)
                {
                    // Sun source needs be a directional light
                    if (currLight == sunLight)
                        return i;

                    // In case no sun light is present we will return the brightest directional light
                    if (currLight.intensity > brightestLightIntensity)
                    {
                        brightestLightIntensity = currLight.intensity;
                        brightestDirectionalLightIndex = i;
                    }
                }
            }

            return brightestDirectionalLightIndex;
        }
        
        internal static void GetPunctualLightDistanceAttenuation(float lightRange, ref Vector4 lightAttenuation)
        {
            // Light attenuation in universal matches the unity vanilla one (HINT_NICE_QUALITY).
            // attenuation = 1.0 / distanceToLightSqr
            // The smoothing factor makes sure that the light intensity is zero at the light range limit.
            // (We used to offer two different smoothing factors.)

            // The current smoothing factor matches the one used in the Unity lightmapper.
            // smoothFactor = (1.0 - saturate((distanceSqr * 1.0 / lightRangeSqr)^2))^2
            float lightRangeSqr = lightRange * lightRange;
            float fadeStartDistanceSqr = 0.8f * 0.8f * lightRangeSqr;
            float fadeRangeSqr = (fadeStartDistanceSqr - lightRangeSqr);
            float lightRangeSqrOverFadeRangeSqr = -lightRangeSqr / fadeRangeSqr;
            float oneOverLightRangeSqr = 1.0f / Mathf.Max(0.0001f, lightRangeSqr);

            // On all devices: Use the smoothing factor that matches the GI.
            lightAttenuation.x = oneOverLightRangeSqr;
            lightAttenuation.y = lightRangeSqrOverFadeRangeSqr;
        }

        internal static void GetSpotAngleAttenuation(
            float spotAngle, float? innerSpotAngle,
            ref Vector4 lightAttenuation)
        {
            // Spot Attenuation with a linear falloff can be defined as
            // (SdotL - cosOuterAngle) / (cosInnerAngle - cosOuterAngle)
            // This can be rewritten as
            // invAngleRange = 1.0 / (cosInnerAngle - cosOuterAngle)
            // SdotL * invAngleRange + (-cosOuterAngle * invAngleRange)
            // If we precompute the terms in a MAD instruction
            float cosOuterAngle = Mathf.Cos(Mathf.Deg2Rad * spotAngle * 0.5f);
            // We need to do a null check for particle lights
            // This should be changed in the future
            // Particle lights will use an inline function
            float cosInnerAngle;
            if (innerSpotAngle.HasValue)
                cosInnerAngle = Mathf.Cos(innerSpotAngle.Value * Mathf.Deg2Rad * 0.5f);
            else
                cosInnerAngle = Mathf.Cos((2.0f * Mathf.Atan(Mathf.Tan(spotAngle * 0.5f * Mathf.Deg2Rad) * (64.0f - 18.0f) / 64.0f)) * 0.5f);
            float smoothAngleRange = Mathf.Max(0.001f, cosInnerAngle - cosOuterAngle);
            float invAngleRange = 1.0f / smoothAngleRange;
            float add = -cosOuterAngle * invAngleRange;

            lightAttenuation.z = invAngleRange;
            lightAttenuation.w = add;
        }

        internal static void GetSpotDirection(ref Matrix4x4 lightLocalToWorldMatrix, out Vector4 lightSpotDir)
        {
            Vector4 dir = lightLocalToWorldMatrix.GetColumn(2);
            lightSpotDir = new Vector4(-dir.x, -dir.y, -dir.z, 0.0f);
        }
        
        /// <summary>
        /// Initializes common light constants.
        /// </summary>
        /// <param name="lights">List of lights to iterate.</param>
        /// <param name="lightIndex">The index of the light.</param>
        /// <param name="lightPos">The position of the light.</param>
        /// <param name="lightColor">The color of the light.</param>
        /// <param name="lightAttenuation">The attenuation of the light.</param>
        /// <param name="lightSpotDir">The direction of the light.</param>
        internal static void InitializeLightConstants(NativeArray<VisibleLight> lights, int lightIndex, out Vector4 lightPos, out Vector4 lightColor, out Vector4 lightAttenuation, out Vector4 lightSpotDir)
        {
            lightPos = k_DefaultLightPosition;
            lightColor = k_DefaultLightColor;
            lightAttenuation = k_DefaultLightAttenuation;  // Directional by default.
            lightSpotDir = k_DefaultLightSpotDirection;

            // When no lights are visible, main light will be set to -1.
            // In this case we initialize it to default values and return
            if (lightIndex < 0)
                return;

            // Avoid memcpys. Pass by ref and locals for multiple uses.
            ref VisibleLight lightData = ref lights.UnsafeElementAtMutable(lightIndex);
            var light = lightData.light;
            var lightLocalToWorld = lightData.localToWorldMatrix;
            var lightType = lightData.lightType;

            if (lightType == LightType.Directional)
            {
                Vector4 dir = -lightLocalToWorld.GetColumn(2);
                lightPos = new Vector4(dir.x, dir.y, dir.z, 0.0f);
            }
            else
            {
                Vector4 pos = lightLocalToWorld.GetColumn(3);
                lightPos = new Vector4(pos.x, pos.y, pos.z, 1.0f);

                GetPunctualLightDistanceAttenuation(lightData.range, ref lightAttenuation);

                if (lightType == LightType.Spot)
                {
                    GetSpotAngleAttenuation(lightData.spotAngle, light?.innerSpotAngle, ref lightAttenuation);
                    GetSpotDirection(ref lightLocalToWorld, out lightSpotDir);
                }
            }

            // VisibleLight.finalColor already returns color in active color space
            lightColor = lightData.finalColor;
        }
    }
}