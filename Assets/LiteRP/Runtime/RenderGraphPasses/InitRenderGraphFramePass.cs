using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder
    { 
        private static readonly ProfilingSampler s_InitRenderGraphFrameProfilingSampler = new ProfilingSampler("InitRenderGraphFramePass");
        internal class InitRenderGraphFramePassData
        {
        }

        private void AddInitRenderGraphFramePass(RenderGraph renderGraph)
        {
            using (var builder = renderGraph.AddUnsafePass<InitRenderGraphFramePassData>("Init RenderGraph Frame Pass", out var passData,
                       s_InitRenderGraphFrameProfilingSampler))
            {
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((InitRenderGraphFramePassData data, UnsafeGraphContext context) =>
                {
                    UnsafeCommandBuffer cmd = context.cmd;
#if UNITY_EDITOR
                    float time = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
#else
                    float time = Time.time;
#endif
                    float deltaTime = Time.deltaTime;
                    float smoothDeltaTime = Time.smoothDeltaTime;

                    ClearRenderingState(cmd);
                    //SetShaderTimeValues(cmd, time, deltaTime, smoothDeltaTime);
                });
            }
        }
        private void ClearRenderingState(IBaseCommandBuffer cmd)
        {
            // Reset per-camera shader keywords. They are enabled depending on which render passes are executed.
            cmd.SetKeyword(ShaderGlobalKeywords.MainLightShadows, false);
            cmd.SetKeyword(ShaderGlobalKeywords.MainLightShadowCascades, false);
            cmd.SetKeyword(ShaderGlobalKeywords.SoftShadows, false);
            cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsLow, false);
            cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsMedium, false);
            cmd.SetKeyword(ShaderGlobalKeywords.SoftShadowsHigh, false);
        }
        private void SetShaderTimeValues(IBaseCommandBuffer cmd, float time, float deltaTime, float smoothDeltaTime)
        {
            /*float timeEights = time / 8f;
            float timeFourth = time / 4f;
            float timeHalf = time / 2f;

            float lastTime = time - ShaderUtils.PersistentDeltaTime;

            // Time values
            Vector4 timeVector = time * new Vector4(1f / 20f, 1f, 2f, 3f);
            Vector4 sinTimeVector = new Vector4(Mathf.Sin(timeEights), Mathf.Sin(timeFourth), Mathf.Sin(timeHalf), Mathf.Sin(time));
            Vector4 cosTimeVector = new Vector4(Mathf.Cos(timeEights), Mathf.Cos(timeFourth), Mathf.Cos(timeHalf), Mathf.Cos(time));
            Vector4 deltaTimeVector = new Vector4(deltaTime, 1f / deltaTime, smoothDeltaTime, 1f / smoothDeltaTime);
            Vector4 timeParametersVector = new Vector4(time, Mathf.Sin(time), Mathf.Cos(time), 0.0f);
            Vector4 lastTimeParametersVector = new Vector4(lastTime, Mathf.Sin(lastTime), Mathf.Cos(lastTime), 0.0f);

            cmd.SetGlobalVector(ShaderPropertyId.time, timeVector);
            cmd.SetGlobalVector(ShaderPropertyId.sinTime, sinTimeVector);
            cmd.SetGlobalVector(ShaderPropertyId.cosTime, cosTimeVector);
            cmd.SetGlobalVector(ShaderPropertyId.deltaTime, deltaTimeVector);
            cmd.SetGlobalVector(ShaderPropertyId.timeParameters, timeParametersVector);
            cmd.SetGlobalVector(ShaderPropertyId.lastTimeParameters, lastTimeParametersVector);*/
        }
    }
}
