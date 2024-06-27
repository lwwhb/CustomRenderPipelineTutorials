using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRPRenderGraphRecorder
    { 
        private static readonly ProfilingSampler s_SetupCameraPropertiesProfilingSampler = new ProfilingSampler("SetupCameraPropertiesPass");
        internal class SetupCameraPropertiesPassData
        {
            internal CameraData cameraData;
        }

        private void AddSetupCameraPropertiesPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<SetupCameraPropertiesPassData>("Setup Camera Properties Pass", out var passData,
                       s_SetupCameraPropertiesProfilingSampler))
            {
                passData.cameraData = cameraData;
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);
                builder.SetRenderFunc((SetupCameraPropertiesPassData data, RasterGraphContext context) =>
                {
                    context.cmd.SetupCameraProperties(data.cameraData.camera);
#if UNITY_EDITOR
                    float time = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
#else
                    float time = Time.time;
#endif
                    float deltaTime = Time.deltaTime;
                    float smoothDeltaTime = Time.smoothDeltaTime;

                    bool yFlip = !SystemInfo.graphicsUVStartsAtTop;
                    // 重置Shader时间变量，因为他们会在SetupCameraPropertiesPass中被覆盖，如果我们不设置，阴影和主渲染可能会不匹配
                    SetShaderTimeValues(context.cmd, time, deltaTime, smoothDeltaTime);
                    SetPerCameraShaderVariables(context.cmd, passData.cameraData, yFlip);
                });
            }
        }

        internal void SetPerCameraShaderVariables(RasterCommandBuffer cmd, CameraData cameraData, bool isTargetFlipped = false)
        {
            Camera camera = cameraData.camera;
            
            float cameraWidth = (float)camera.pixelWidth;
            float cameraHeight = (float)camera.pixelHeight;

            float near = camera.nearClipPlane;
            float far = camera.farClipPlane;
            float invNear = Mathf.Approximately(near, 0.0f) ? 0.0f : 1.0f / near;
            float invFar = Mathf.Approximately(far, 0.0f) ? 0.0f : 1.0f / far;
            float isOrthographic = camera.orthographic ? 1.0f : 0.0f;

            // From http://www.humus.name/temp/Linearize%20depth.txt
            // But as depth component textures on OpenGL always return in 0..1 range (as in D3D), we have to use
            // the same constants for both D3D and OpenGL here.
            // OpenGL would be this:
            // zc0 = (1.0 - far / near) / 2.0;
            // zc1 = (1.0 + far / near) / 2.0;
            // D3D is this:
            float zc0 = 1.0f - far * invNear;
            float zc1 = far * invNear;

            Vector4 zBufferParams = new Vector4(zc0, zc1, zc0 * invFar, zc1 * invFar);

            if (SystemInfo.usesReversedZBuffer)
            {
                zBufferParams.y += zBufferParams.x;
                zBufferParams.x = -zBufferParams.x;
                zBufferParams.w += zBufferParams.z;
                zBufferParams.z = -zBufferParams.z;
            }

            // Projection flip sign logic is very deep in GfxDevice::SetInvertProjectionMatrix
            // This setup is tailored especially for overlay camera game view
            // For other scenarios this will be overwritten correctly by SetupCameraProperties
            float projectionFlipSign = isTargetFlipped ? -1.0f : 1.0f;
            Vector4 projectionParams = new Vector4(projectionFlipSign, near, far, 1.0f * invFar);
            cmd.SetGlobalVector(ShaderPropertyId.projectionParams, projectionParams);

            float aspectRatio = cameraData.GetCameraAspectRatio();

            Vector4 orthoParams = new Vector4(camera.orthographicSize * aspectRatio, camera.orthographicSize, 0.0f, isOrthographic);

            // Camera and Screen variables as described in https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
            cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, camera.transform.position);
            cmd.SetGlobalVector(ShaderPropertyId.screenParams, new Vector4(cameraWidth, cameraHeight, 1.0f + 1.0f / cameraWidth, 1.0f + 1.0f / cameraHeight));
            cmd.SetGlobalVector(ShaderPropertyId.zBufferParams, zBufferParams);
            cmd.SetGlobalVector(ShaderPropertyId.orthoParams, orthoParams);

            //Set per camera matrices.
            //SetCameraMatrices(cmd, cameraData, true, isTargetFlipped);
        }
        
        /*internal static void SetCameraMatrices(RasterCommandBuffer cmd, CameraData cameraData, bool setInverseMatrices, bool isTargetFlipped)
        {
            // NOTE: the URP default main view/projection matrices are the CameraData view/projection matrices.
            Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
            Matrix4x4 projectionMatrix = cameraData.GetProjectionMatrix(); // Jittered, non-gpu

            // Set the default view/projection, note: projectionMatrix will be set as a gpu-projection (gfx api adjusted) for rendering.
            cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

            if (setInverseMatrices)
            {
                Matrix4x4 gpuProjectionMatrix = cameraData.GetGPUProjectionMatrix(isTargetFlipped); // TODO: invProjection might NOT match the actual projection (invP*P==I) as the target flip logic has diverging paths.
                Matrix4x4 inverseViewMatrix = Matrix4x4.Inverse(viewMatrix);
                Matrix4x4 inverseProjectionMatrix = Matrix4x4.Inverse(gpuProjectionMatrix);
                Matrix4x4 inverseViewProjection = inverseViewMatrix * inverseProjectionMatrix;

                // There's an inconsistency in handedness between unity_matrixV and unity_WorldToCamera
                // Unity changes the handedness of unity_WorldToCamera (see Camera::CalculateMatrixShaderProps)
                // we will also change it here to avoid breaking existing shaders. (case 1257518)
                Matrix4x4 worldToCameraMatrix = Matrix4x4.Scale(new Vector3(1.0f, 1.0f, -1.0f)) * viewMatrix;
                Matrix4x4 cameraToWorldMatrix = worldToCameraMatrix.inverse;
                cmd.SetGlobalMatrix(ShaderPropertyId.worldToCameraMatrix, worldToCameraMatrix);
                cmd.SetGlobalMatrix(ShaderPropertyId.cameraToWorldMatrix, cameraToWorldMatrix);

                cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewMatrix, inverseViewMatrix);
                cmd.SetGlobalMatrix(ShaderPropertyId.inverseProjectionMatrix, inverseProjectionMatrix);
                cmd.SetGlobalMatrix(ShaderPropertyId.inverseViewAndProjectionMatrix, inverseViewProjection);
            }
        }*/
    }
}