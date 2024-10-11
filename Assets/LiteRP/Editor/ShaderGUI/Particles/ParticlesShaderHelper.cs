using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Editor
{
    internal static class ParticlesShaderHelper 
    {
        /// <summary>
        /// The available color modes.
        /// Controls how the Particle color and the Material color blend together.
        /// </summary>
        public enum ColorMode
        {
            /// <summary>
            /// Use this to select multiply mode.
            /// </summary>
            Multiply,

            /// <summary>
            /// Use this to select additive mode.
            /// </summary>
            Additive,

            /// <summary>
            /// Use this to select subtractive mode.
            /// </summary>
            Subtractive,

            /// <summary>
            /// Use this to select overlay mode.
            /// </summary>
            Overlay,

            /// <summary>
            /// Use this to select color mode.
            /// </summary>
            Color,

            /// <summary>
            /// Use this to select difference mode.
            /// </summary>
            Difference
        }
        /// <summary>
        /// Sets up the material with correct keywords based on the color mode.
        /// </summary>
        /// <param name="material">The material to use.</param>
        public static void SetupMaterialWithColorMode(Material material)
        {
            var colorMode = (ColorMode)material.GetFloat("_ColorMode");

            switch (colorMode)
            {
                case ColorMode.Multiply:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.DisableKeyword("_COLORADDSUBDIFF_ON");
                    break;
                case ColorMode.Overlay:
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.DisableKeyword("_COLORADDSUBDIFF_ON");
                    material.EnableKeyword("_COLOROVERLAY_ON");
                    break;
                case ColorMode.Color:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORADDSUBDIFF_ON");
                    material.EnableKeyword("_COLORCOLOR_ON");
                    break;
                case ColorMode.Difference:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.EnableKeyword("_COLORADDSUBDIFF_ON");
                    material.SetVector("_BaseColorAddSubDiff", new Vector4(-1.0f, 1.0f, 0.0f, 0.0f));
                    break;
                case ColorMode.Additive:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.EnableKeyword("_COLORADDSUBDIFF_ON");
                    material.SetVector("_BaseColorAddSubDiff", new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
                    break;
                case ColorMode.Subtractive:
                    material.DisableKeyword("_COLOROVERLAY_ON");
                    material.DisableKeyword("_COLORCOLOR_ON");
                    material.EnableKeyword("_COLORADDSUBDIFF_ON");
                    material.SetVector("_BaseColorAddSubDiff", new Vector4(-1.0f, 0.0f, 0.0f, 0.0f));
                    break;
            }
        }
        /// <summary>
        /// Sets up the keywords for the material and shader.
        /// </summary>
        /// <param name="material">The material to use.</param>
        public static void SetMaterialKeywords(Material material)
        {
            // Setup particle + material color blending
            SetupMaterialWithColorMode(material);
            // Is the material transparent, this is set in BaseShaderGUI
            bool isTransparent = material.GetTag("RenderType", false) == "Transparent";
            // Z write doesn't work with distortion/fading
            bool hasZWrite = (material.GetFloat(LiteRPShaderProperty.ZWrite) > 0.0f);

            // Flipbook blending
            if (material.HasProperty(LiteRPShaderProperty.FlipbookMode))
            {
                var useFlipbookBlending = (material.GetFloat(LiteRPShaderProperty.FlipbookMode) > 0.0f);
                CoreUtils.SetKeyword(material, "_FLIPBOOKBLENDING_ON", useFlipbookBlending);
            }
            // Soft particles
            var useSoftParticles = false;
            if (material.HasProperty(LiteRPShaderProperty.SoftParticlesEnabled))
            {
                useSoftParticles = (material.GetFloat(LiteRPShaderProperty.SoftParticlesEnabled) > 0.0f && isTransparent);
                if (useSoftParticles)
                {
                    var softParticlesNearFadeDistance = material.GetFloat(LiteRPShaderProperty.SoftParticlesNearFadeDistance);
                    var softParticlesFarFadeDistance = material.GetFloat(LiteRPShaderProperty.SoftParticlesFarFadeDistance);
                    // clamp values
                    if (softParticlesNearFadeDistance < 0.0f)
                    {
                        softParticlesNearFadeDistance = 0.0f;
                        material.SetFloat(LiteRPShaderProperty.SoftParticlesNearFadeDistance, 0.0f);
                    }

                    if (softParticlesFarFadeDistance < 0.0f)
                    {
                        softParticlesFarFadeDistance = 0.0f;
                        material.SetFloat(LiteRPShaderProperty.SoftParticlesFarFadeDistance, 0.0f);
                    }
                    // set keywords
                    material.SetVector(LiteRPShaderProperty.SoftParticleFadeParams,
                        new Vector4(softParticlesNearFadeDistance,
                            1.0f / (softParticlesFarFadeDistance - softParticlesNearFadeDistance), 0.0f, 0.0f));
                }
                else
                {
                    material.SetVector(LiteRPShaderProperty.SoftParticleFadeParams, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
                }
                CoreUtils.SetKeyword(material, "_SOFTPARTICLES_ON", useSoftParticles);
            }
            // Camera fading
            var useCameraFading = false;
            if (material.HasProperty(LiteRPShaderProperty.CameraFadingEnabled) && isTransparent)
            {
                useCameraFading = (material.GetFloat(LiteRPShaderProperty.CameraFadingEnabled) > 0.0f);
                if (useCameraFading)
                {
                    var cameraNearFadeDistance = material.GetFloat(LiteRPShaderProperty.CameraNearFadeDistance);
                    var cameraFarFadeDistance = material.GetFloat(LiteRPShaderProperty.CameraFarFadeDistance);
                    // clamp values
                    if (cameraNearFadeDistance < 0.0f)
                    {
                        cameraNearFadeDistance = 0.0f;
                        material.SetFloat(LiteRPShaderProperty.CameraNearFadeDistance, 0.0f);
                    }

                    if (cameraFarFadeDistance < 0.0f)
                    {
                        cameraFarFadeDistance = 0.0f;
                        material.SetFloat(LiteRPShaderProperty.CameraFarFadeDistance, 0.0f);
                    }
                    // set keywords
                    material.SetVector(LiteRPShaderProperty.CameraFadeParams,
                        new Vector4(cameraNearFadeDistance, 1.0f / (cameraFarFadeDistance - cameraNearFadeDistance),
                            0.0f, 0.0f));
                }
                else
                {
                    material.SetVector(LiteRPShaderProperty.CameraFadeParams, new Vector4(0.0f, Mathf.Infinity, 0.0f, 0.0f));
                }
            }
            // Distortion
            if (material.HasProperty(LiteRPShaderProperty.DistortionEnabled))
            {
                var useDistortion = (material.GetFloat(LiteRPShaderProperty.DistortionEnabled) > 0.0f) && isTransparent;
                CoreUtils.SetKeyword(material, "_DISTORTION_ON", useDistortion);
                if (useDistortion)
                    material.SetFloat(LiteRPShaderProperty.DistortionStrengthScaled, material.GetFloat(LiteRPShaderProperty.DistortionStrength) * 0.1f);
            }

            var useFading = (useSoftParticles || useCameraFading) && !hasZWrite;
            CoreUtils.SetKeyword(material, "_FADING_ON", useFading);
        }
    }
}
