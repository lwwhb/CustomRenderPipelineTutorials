using LiteRP.FrameData;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_SetupLightsProfilingSampler = new ProfilingSampler("SetupLightsPass");
        
        private Vector4[] m_AdditionalLightPositions;
        private Vector4[] m_AdditionalLightColors;
        private Vector4[] m_AdditionalLightAttenuations;
        private Vector4[] m_AdditionalLightSpotDirections;
        
        internal class SetupLightsPassData
        {
            internal CameraData cameraData;
            internal LightData lightData;
        }
        
        
        private void AddSetupLightsPass(RenderGraph renderGraph, CameraData cameraData, LightData lightData)
        {
            using (var builder = renderGraph.AddUnsafePass<SetupLightsPassData>("Setup Lights Pass", out var passData,
                       s_SetupLightsProfilingSampler))
            {
                passData.cameraData = cameraData;
                passData.lightData = lightData;

                builder.AllowPassCulling(false);

                builder.SetRenderFunc((SetupLightsPassData data, UnsafeGraphContext rgContext) =>
                {
                    SetLightsShaderVariables(rgContext.cmd, data.cameraData, data.lightData);
                });
            }
        }

        private void SetLightsShaderVariables(UnsafeCommandBuffer cmd, CameraData cameraData, LightData lightData)
        {
            int additionalLightsCount = lightData.additionalLightsCount;
            SetupMainLightConstants(cmd, lightData);
            SetupAdditionalLightConstants(cmd, ref cameraData.cullingResults, lightData);

            bool lightCountCheck =  additionalLightsCount > 0;  //lwwhb 可能添加其他条件，如是否剔除了多光源Shader变体
            cmd.SetKeyword(ShaderGlobalKeywords.AdditionalLights,  lightCountCheck);

            //cmd.SetKeyword(ShaderGlobalKeywords.ReflectionProbeBlending, lightData.reflectionProbeBlending);
            //cmd.SetKeyword(ShaderGlobalKeywords.ReflectionProbeBoxProjection, lightData.reflectionProbeBoxProjection);

                //var asset = LiteRenderPipeline.asset;
                //bool apvIsEnabled = asset != null && asset.lightProbeSystem == LightProbeSystem.ProbeVolumes;
                //ProbeVolumeSHBands probeVolumeSHBands = asset.probeVolumeSHBands;

                //cmd.SetKeyword(ShaderGlobalKeywords.ProbeVolumeL1, apvIsEnabled && probeVolumeSHBands == ProbeVolumeSHBands.SphericalHarmonicsL1);
                //cmd.SetKeyword(ShaderGlobalKeywords.ProbeVolumeL2, apvIsEnabled && probeVolumeSHBands == ProbeVolumeSHBands.SphericalHarmonicsL2);

				// TODO: If we can robustly detect LIGHTMAP_ON, we can skip SH logic.
                //var shMode = PlatformAutoDetect.ShAutoDetect(asset.shEvalMode);
                //cmd.SetKeyword(ShaderGlobalKeywords.EVALUATE_SH_MIXED, shMode == ShEvalMode.Mixed);
                //cmd.SetKeyword(ShaderGlobalKeywords.EVALUATE_SH_VERTEX, shMode == ShEvalMode.PerVertex);

                //var stack = VolumeManager.instance.stack;
                /*bool enableProbeVolumes = ProbeReferenceVolume.instance.UpdateShaderVariablesProbeVolumes(
                    CommandBufferHelpers.GetNativeCommandBuffer(cmd),
                    stack.GetComponent<ProbeVolumesOptions>(),
                    cameraData.IsTemporalAAEnabled() ? Time.frameCount : 0,
                    lightData.supportsLightLayers);*/

                //cmd.SetGlobalInt("_EnableProbeVolumes", enableProbeVolumes ? 1 : 0);
                //cmd.SetKeyword(ShaderGlobalKeywords.LightLayers, lightData.supportsLightLayers && !CoreUtils.IsSceneLightingDisabled(cameraData.camera));


                //cmd.SetKeyword(ShaderGlobalKeywords.LightCookies, false);
        }

        private void SetupMainLightConstants(UnsafeCommandBuffer cmd, LightData lightData)
        {
            Vector4 lightPos, lightColor, lightAttenuation, lightSpotDir;
            LightUtils.InitializeLightConstants(lightData.visibleLights, lightData.mainLightIndex, out lightPos, out lightColor, out lightAttenuation, out lightSpotDir);
            lightColor.w = 1f;

            cmd.SetGlobalVector(ShaderPropertyId.mainLightPosition, lightPos);
            cmd.SetGlobalVector(ShaderPropertyId.mainLightColor, lightColor);
        }

        void SetupAdditionalLightConstants(UnsafeCommandBuffer cmd, ref CullingResults cullResults, LightData lightData)
        {
            var lights = lightData.visibleLights;
            int maxAdditionalLightsCount = LightUtils.maxVisibleAdditionalLights;
            int additionalLightsCount = SetupPerObjectLightIndices(cullResults, lightData);
            if (additionalLightsCount > 0)
            {
                for (int i = 0, lightIter = 0; i < lights.Length && lightIter < maxAdditionalLightsCount; ++i)
                {
                    if (lightData.mainLightIndex != i)
                    {
                        LightUtils.InitializeLightConstants(
                            lights,
                            i,
                            out m_AdditionalLightPositions[lightIter],
                            out m_AdditionalLightColors[lightIter],
                            out m_AdditionalLightAttenuations[lightIter],
                            out m_AdditionalLightSpotDirections[lightIter]);
                        m_AdditionalLightColors[lightIter].w = 0f;
                        lightIter++;
                    }
                }

                cmd.SetGlobalVectorArray(ShaderPropertyId.additionalLightsPosition, m_AdditionalLightPositions);
                cmd.SetGlobalVectorArray(ShaderPropertyId.additionalLightsColor, m_AdditionalLightColors);
                cmd.SetGlobalVectorArray(ShaderPropertyId.additionalLightsAttenuation, m_AdditionalLightAttenuations);
                cmd.SetGlobalVectorArray(ShaderPropertyId.additionalLightsSpotDir, m_AdditionalLightSpotDirections);

                cmd.SetGlobalVector(ShaderPropertyId.additionalLightsCount, new Vector4(lightData.maxPerObjectAdditionalLightsCount, 0.0f, 0.0f, 0.0f));
            }
            else
            {
                cmd.SetGlobalVector(ShaderPropertyId.additionalLightsCount, Vector4.zero);
            }
        }
        
        int SetupPerObjectLightIndices(CullingResults cullResults, LightData lightData)
        {
            if (lightData.additionalLightsCount == 0)
                return lightData.additionalLightsCount;

            var perObjectLightIndexMap = cullResults.GetLightIndexMap(Allocator.Temp);
            int globalDirectionalLightsCount = 0;
            int additionalLightsCount = 0;

            // Disable all directional lights from the perobject light indices
            // Pipeline handles main light globally and there's no support for additional directional lights atm.
            int maxVisibleAdditionalLightsCount = LightUtils.maxVisibleAdditionalLights;
            int len = lightData.visibleLights.Length;
            for (int i = 0; i < len; ++i)
            {
                if (additionalLightsCount >= maxVisibleAdditionalLightsCount)
                    break;

                if (i == lightData.mainLightIndex)
                {
                    perObjectLightIndexMap[i] = -1;
                    ++globalDirectionalLightsCount;
                }
                else
                {
                    if (lightData.visibleLights[i].lightType == LightType.Directional ||
                        lightData.visibleLights[i].lightType == LightType.Spot ||
                        lightData.visibleLights[i].lightType == LightType.Point)
                    {
                        // Light type is supported
                        perObjectLightIndexMap[i] -= globalDirectionalLightsCount;
                    }
                    else
                    {
                        // Light type is not supported. Skip the light.
                        perObjectLightIndexMap[i] = -1;
                    }

                    ++additionalLightsCount;
                }
            }

            // Disable all remaining lights we cannot fit into the global light buffer.
            for (int i = globalDirectionalLightsCount + additionalLightsCount; i < perObjectLightIndexMap.Length; ++i)
                perObjectLightIndexMap[i] = -1;

            cullResults.SetLightIndexMap(perObjectLightIndexMap);
            
            perObjectLightIndexMap.Dispose();
            return additionalLightsCount;
        }
    }
}