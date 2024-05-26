using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace LiteRP.AdditionalData
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [ImageEffectAllowedInSceneView]
    [ExecuteAlways]
    public class AdditionalCameraData : MonoBehaviour, ISerializationCallbackReceiver, IAdditionalData
    {
        /// <summary>
        /// Returns true if this camera should render post-processing.
        /// </summary>
        [SerializeField] bool m_RenderPostProcessing = false;
        public bool renderPostProcessing
        {
            get => m_RenderPostProcessing;
            set => m_RenderPostProcessing = value;
        }
        /// <summary>
        /// Returns true if this camera should automatically replace NaN/Inf in shaders by a black pixel to avoid breaking some effects.
        /// </summary>
        /// 
        [SerializeField] bool m_StopNaN = false;
        public bool stopNaN
        {
            get => m_StopNaN;
            set => m_StopNaN = value;
        }
        /// <summary>
        /// Returns true if this camera applies 8-bit dithering to the final render to reduce color banding
        /// </summary>
        [SerializeField] bool m_Dithering = false;
        public bool dithering
        {
            get => m_Dithering;
            set => m_Dithering = value;
        }
        
        /// <summary>
        /// Controls if this camera should render shadows.
        /// </summary>
        [SerializeField] bool m_RenderShadows = true;
        public bool renderShadows
        {
            get => m_RenderShadows;
            set => m_RenderShadows = value;
        }
        
        /// <summary>
        /// Returns the selected scene-layers affecting this camera.
        /// </summary>
        [SerializeField] LayerMask m_VolumeLayerMask = 1; // "Default"
        public LayerMask volumeLayerMask
        {
            get => m_VolumeLayerMask;
            set => m_VolumeLayerMask = value;
        }

        /// <summary>
        /// Returns the Transform that acts as a trigger for Volume blending.
        /// </summary>
        [SerializeField] Transform m_VolumeTrigger = null;
        public Transform volumeTrigger
        {
            get => m_VolumeTrigger;
            set => m_VolumeTrigger = value;
        }

        /// <summary>
        /// Returns the selected mode for Volume Frame Updates.
        /// </summary>
        [SerializeField] VolumeFrameworkUpdateMode m_VolumeFrameworkUpdateModeOption = VolumeFrameworkUpdateMode.UsePipelineSettings;
        internal VolumeFrameworkUpdateMode volumeFrameworkUpdateMode
        {
            get => m_VolumeFrameworkUpdateModeOption;
            set => m_VolumeFrameworkUpdateModeOption = value;
        }

        /// <summary>
        /// Returns true if this camera requires the volume framework to be updated every frame.
        /// </summary>
        public bool requiresVolumeFrameworkUpdate
        {
            get
            {
                if (m_VolumeFrameworkUpdateModeOption == VolumeFrameworkUpdateMode.UsePipelineSettings)
                {
                    return LiteRenderPipeline.asset.volumeFrameworkUpdateMode != VolumeFrameworkUpdateMode.ViaScripting;
                }

                return m_VolumeFrameworkUpdateModeOption == VolumeFrameworkUpdateMode.EveryFrame;
            }
        }
        
        /// <summary>
        /// Container for volume stacks in order to reuse stacks and avoid
        /// creating new ones every time a new camera is instantiated.
        /// </summary>
        private static List<VolumeStack> s_CachedVolumeStacks;

        /// <summary>
        /// Returns the current volume stack used by this camera.
        /// </summary>
        public VolumeStack volumeStack
        {
            get => m_VolumeStack;
            set
            {
                // If the volume stack is being removed,
                // add it back to the list so it can be reused later
                if (value == null && m_VolumeStack != null && m_VolumeStack.isValid)
                {
                    if (s_CachedVolumeStacks == null)
                        s_CachedVolumeStacks = new List<VolumeStack>(4);

                    s_CachedVolumeStacks.Add(m_VolumeStack);
                }

                m_VolumeStack = value;
            }
        }
        VolumeStack m_VolumeStack = null;

        /// <summary>
        /// Tries to retrieve a volume stack from the container
        /// and creates a new one if that fails.
        /// </summary>
        internal void GetOrCreateVolumeStack()
        {
            // Try first to reuse a volume stack
            if (s_CachedVolumeStacks != null && s_CachedVolumeStacks.Count > 0)
            {
                int index = s_CachedVolumeStacks.Count - 1;
                var stack = s_CachedVolumeStacks[index];
                s_CachedVolumeStacks.RemoveAt(index);
                if (stack.isValid)
                    volumeStack = stack;
            }

            // Create a new stack if was not possible to reuse an old one
            if (volumeStack == null)
                volumeStack = VolumeManager.instance.CreateStack();
        }
        
        [NonSerialized] Camera m_Camera;
#if UNITY_EDITOR
        internal new Camera camera
#else
        internal Camera camera
#endif
        {
            get
            {
                if (!m_Camera)
                {
                    gameObject.TryGetComponent<Camera>(out m_Camera);
                }
                return m_Camera;
            }
        }
        
        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            
        }
    }
    
    /// <summary>
    /// Contains extension methods for Camera class.
    /// </summary>
    public static class CameraExtensions
    {
        /// <summary>
        /// Lite Render Pipeline exposes additional rendering data in a separate component.
        /// This method returns the additional data component for the given camera or create one if it doesn't exist yet.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns>The <c>UniversalAdditionalCameraData</c> for this camera.</returns>
        /// <see cref="AdditionalCameraData"/>
        public static AdditionalCameraData GetAdditionalCameraData(this Camera camera)
        {
            var gameObject = camera.gameObject;
            bool componentExists = gameObject.TryGetComponent<AdditionalCameraData>(out var cameraData);
            if (!componentExists)
                cameraData = gameObject.AddComponent<AdditionalCameraData>();

            return cameraData;
        }

        /// <summary>
        /// Returns the VolumeFrameworkUpdateMode set on the camera.
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static VolumeFrameworkUpdateMode GetVolumeFrameworkUpdateMode(this Camera camera)
        {
            AdditionalCameraData cameraData = camera.GetAdditionalCameraData();
            return cameraData.volumeFrameworkUpdateMode;
        }

        /// <summary>
        /// Sets the VolumeFrameworkUpdateMode for the camera.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="mode"></param>
        public static void SetVolumeFrameworkUpdateMode(this Camera camera, VolumeFrameworkUpdateMode mode)
        {
            AdditionalCameraData cameraData = camera.GetAdditionalCameraData();
            if (cameraData.volumeFrameworkUpdateMode == mode)
                return;

            bool requiredUpdatePreviously = cameraData.requiresVolumeFrameworkUpdate;
            cameraData.volumeFrameworkUpdateMode = mode;

            // We only update the local volume stacks for cameras set to ViaScripting.
            // Otherwise it will be updated in every frame.
            // We also check the previous value to make sure we're not updating when
            // switching between Camera ViaScripting and the URP Asset set to ViaScripting
            if (requiredUpdatePreviously && !cameraData.requiresVolumeFrameworkUpdate)
                camera.UpdateVolumeStack(cameraData);
        }

        /// <summary>
        /// Updates the volume stack for this camera.
        /// This function will only update the stack when the camera has VolumeFrameworkUpdateMode set to ViaScripting
        /// or when it set to UsePipelineSettings and the update mode on the Render Pipeline Asset is set to ViaScripting.
        /// </summary>
        /// <param name="camera"></param>
        public static void UpdateVolumeStack(this Camera camera)
        {
            AdditionalCameraData cameraData = camera.GetAdditionalCameraData();
            camera.UpdateVolumeStack(cameraData);
        }

        /// <summary>
        /// Updates the volume stack for this camera.
        /// This function will only update the stack when the camera has ViaScripting selected or if
        /// the camera is set to UsePipelineSettings and the Render Pipeline Asset is set to ViaScripting.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="cameraData"></param>
        public static void UpdateVolumeStack(this Camera camera, AdditionalCameraData cameraData)
        {
            Assert.IsNotNull(cameraData, "cameraData can not be null when updating the volume stack.");

            // We only update the local volume stacks for cameras set to ViaScripting.
            // Otherwise it will be updated in the frame.
            if (cameraData.requiresVolumeFrameworkUpdate)
                return;

            // Create stack for camera
            if (cameraData.volumeStack == null)
                cameraData.GetOrCreateVolumeStack();

            camera.GetVolumeLayerMaskAndTrigger(cameraData, out LayerMask layerMask, out Transform trigger);
            VolumeManager.instance.Update(cameraData.volumeStack, trigger, layerMask);
        }

        /// <summary>
        /// Destroys the volume stack for this camera.
        /// </summary>
        /// <param name="camera"></param>
        public static void DestroyVolumeStack(this Camera camera)
        {
            AdditionalCameraData cameraData = camera.GetAdditionalCameraData();
            camera.DestroyVolumeStack(cameraData);
        }

        /// <summary>
        /// Destroys the volume stack for this camera.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="cameraData"></param>
        public static void DestroyVolumeStack(this Camera camera, AdditionalCameraData cameraData)
        {
            if (cameraData == null || cameraData.volumeStack == null)
                return;

            cameraData.volumeStack = null;
        }

        /// <summary>
        /// Returns the mask and trigger assigned for volumes on the camera.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="cameraData"></param>
        /// <param name="layerMask"></param>
        /// <param name="trigger"></param>
        internal static void GetVolumeLayerMaskAndTrigger(this Camera camera, AdditionalCameraData cameraData, out LayerMask layerMask, out Transform trigger)
        {
            // Default values when there's no additional camera data available
            layerMask = 1; // "Default"
            trigger = camera.transform;

            if (cameraData != null)
            {
                layerMask = cameraData.volumeLayerMask;
                trigger = (cameraData.volumeTrigger != null) ? cameraData.volumeTrigger : trigger;
            }
            else if (camera.cameraType == CameraType.SceneView)
            {
                // Try to mirror the MainCamera volume layer mask for the scene view - do not mirror the target
                var mainCamera = Camera.main;
                AdditionalCameraData mainAdditionalCameraData = null;

                if (mainCamera != null && mainCamera.TryGetComponent(out mainAdditionalCameraData))
                {
                    layerMask = mainAdditionalCameraData.volumeLayerMask;
                }

                trigger = (mainAdditionalCameraData != null && mainAdditionalCameraData.volumeTrigger != null) ? mainAdditionalCameraData.volumeTrigger : trigger;
            }
        }
    }

}