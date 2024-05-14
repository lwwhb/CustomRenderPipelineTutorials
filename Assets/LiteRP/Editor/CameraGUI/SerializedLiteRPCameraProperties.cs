using LiteRP.AdditionalData;
using UnityEditor;
using UnityEditor.Rendering;

namespace LiteRP.Editor
{
    public class SerializedLiteRPCameraProperties : ISerializedCamera
    {
        public SerializedObject serializedObject { get; }
        public SerializedObject serializedAdditionalDataObject { get; }
        public CameraEditor.Settings baseCameraSettings { get; }
        
        public AdditionalCameraData[] camerasAdditionalData { get; }

        // Common properties
        public SerializedProperty projectionMatrixMode { get; }
        public SerializedProperty dithering { get; }
        public SerializedProperty stopNaNs { get; }
        public SerializedProperty allowDynamicResolution { get; }
        public SerializedProperty volumeLayerMask { get; }
        public SerializedProperty clearDepth { get; }
        public SerializedProperty antialiasing { get; }
        
        // LiteRP specific properties
        public SerializedProperty renderShadows { get; }
        public SerializedProperty volumeTrigger { get; }
        public SerializedProperty volumeFrameworkUpdateMode { get; }
        public SerializedProperty renderPostProcessing { get; }
        public SerializedProperty antialiasingQuality { get; }
        public SerializedProperty allowHDROutput { get; }
        public SerializedLiteRPCameraProperties(SerializedObject serializedObject, CameraEditor.Settings settings)
        {
            this.baseCameraSettings = settings;
            settings.OnEnable();

            this.serializedObject = serializedObject;
            projectionMatrixMode = serializedObject.FindProperty("m_projectionMatrixMode");
            allowDynamicResolution = serializedObject.FindProperty("m_AllowDynamicResolution");

            camerasAdditionalData = CoreEditorUtils
                .GetAdditionalData<AdditionalCameraData>(serializedObject.targetObjects);
            serializedAdditionalDataObject = new SerializedObject(camerasAdditionalData);
            
            // Common properties
            stopNaNs = serializedAdditionalDataObject.FindProperty("m_StopNaN");
            dithering = serializedAdditionalDataObject.FindProperty("m_Dithering");
            antialiasing = serializedAdditionalDataObject.FindProperty("m_Antialiasing");
            volumeLayerMask = serializedAdditionalDataObject.FindProperty("m_VolumeLayerMask");
            clearDepth = serializedAdditionalDataObject.FindProperty("m_ClearDepth");
            
            // LiteRP specific properties
            renderShadows = serializedAdditionalDataObject.FindProperty("m_RenderShadows");
            volumeTrigger = serializedAdditionalDataObject.FindProperty("m_VolumeTrigger");
            volumeFrameworkUpdateMode = serializedAdditionalDataObject.FindProperty("m_VolumeFrameworkUpdateModeOption");
            renderPostProcessing = serializedAdditionalDataObject.FindProperty("m_RenderPostProcessing");
            antialiasingQuality = serializedAdditionalDataObject.FindProperty("m_AntialiasingQuality");

            allowHDROutput = serializedAdditionalDataObject.FindProperty("m_AllowHDROutput");
            settings.ApplyModifiedProperties();
        }
        
        public void Update()
        {
            baseCameraSettings.Update();
            serializedObject.Update();
            serializedAdditionalDataObject.Update();
        }

        public void Apply()
        {
            baseCameraSettings.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
            serializedAdditionalDataObject.ApplyModifiedProperties();
        }
        
        public void Refresh()
        {
            
        }
    }
}