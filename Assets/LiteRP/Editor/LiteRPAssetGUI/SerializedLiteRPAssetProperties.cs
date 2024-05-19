using UnityEditor;
using UnityEditor.Rendering;

namespace LiteRP.Editor
{
    internal static class LiteRPAssetProperty
    {
        // RenderPipeline Settings
        public static readonly string UseSRPBatcher = "m_UseSRPBatcher";
        public static readonly string UseGPUResidentDrawer = "m_GPUResidentDrawerMode";
        public static readonly string UseSmallMeshScreenPercentage = "m_SmallMeshScreenPercentage";
        public static readonly string UseGPUResidentDrawerEnableOcclusionCullingInCameras = "m_GPUResidentDrawerEnableOcclusionCullingInCameras";
        
        // Quality Settings
        public static readonly string AntiAliasing = "m_AntiAliasing";
        
        // Shadow Settings
        public static readonly string MainLightShadowEnabled = "m_MainLightShadowEnabled";
        public static readonly string MainLightShadowmapResolution = "m_MainLightShadowmapResolution";
        public static readonly string MainLightShadowDistance = "m_MainLightShadowDistance";
        public static readonly string MainLightShadowCascadesCount = "m_MainLightShadowCascadesCount";
        public static readonly string MainLightShadowCascades2Split = "m_MainLightCascade2Split";
        public static readonly string MainLightShadowCascades3Split = "m_MainLightCascade3Split";
        public static readonly string MainLightShadowCascades4Split = "m_MainLightCascade4Split";
        public static readonly string MainLightShadowCascadesBorder = "m_MainLightCascadeBorder";
        public static readonly string MainLightShadowDepthBias = "m_MainLightShadowDepthBias";
        public static readonly string MainLightShadowNormalBias = "m_MainLightShadowNormalBias";
        
        public static readonly string SupportsSoftShadows = "m_SoftShadowsSupported";
        public static readonly string SoftShadowQuality = "m_SoftShadowQuality";
        
        // Other Settings
        public static readonly string VolumeFrameworkUpdateMode = "m_VolumeFrameworkUpdateMode";
        public static readonly string VolumeProfile = "m_VolumeProfile";
    }
    internal class SerializedLiteRPAssetProperties
    {
        public LiteRPAsset asset { get; }
        public SerializedObject serializedObject { get; }
        
        // RenderPipeline Settings
        public SerializedProperty srpBatcher { get; }
        public SerializedProperty gpuResidentDrawerMode { get; }
        public SerializedProperty smallMeshScreenPercentage { get; }
        public SerializedProperty gpuResidentDrawerEnableOcclusionCullingInCameras { get; }
        
        // Quality Settings
        public SerializedProperty antiAliasing { get; }
        
        // Shadow Settings
        public SerializedProperty mainLightShadowEnabled { get; }
        public SerializedProperty mainLightShadowmapResolution { get; }
        public SerializedProperty mainLightShadowDistance { get; }
        public SerializedProperty mainLightShadowCascadesCount { get; }
        public SerializedProperty mainLightShadowCascade2Split { get; }
        public SerializedProperty mainLightShadowCascade3Split { get; }
        public SerializedProperty mainLightShadowCascade4Split { get; }
        public SerializedProperty mainLightShadowCascadeBorder { get; }
        public SerializedProperty mainLightShadowDepthBias { get; }
        public SerializedProperty mainLightShadowNormalBias { get; }
        
        public SerializedProperty supportsSoftShadows { get; }
        public SerializedProperty softShadowQuality { get; }
        
        // Other Settings
        public EditorPrefBoolFlags<EditorUtils.Unit> state;
        
        public SerializedProperty volumeFrameworkUpdateModeProp { get; }
        public SerializedProperty volumeProfileProp { get; }
        
        public SerializedLiteRPAssetProperties(SerializedObject serializedObject)
        {
            asset = serializedObject.targetObject as LiteRPAsset;
            this.serializedObject = serializedObject;
            
            // RenderPipeline Settings
            srpBatcher = serializedObject.FindProperty(LiteRPAssetProperty.UseSRPBatcher);
            gpuResidentDrawerMode = serializedObject.FindProperty(LiteRPAssetProperty.UseGPUResidentDrawer);
            smallMeshScreenPercentage = serializedObject.FindProperty(LiteRPAssetProperty.UseSmallMeshScreenPercentage);
            gpuResidentDrawerEnableOcclusionCullingInCameras = serializedObject.FindProperty(LiteRPAssetProperty.UseGPUResidentDrawerEnableOcclusionCullingInCameras);
            
            // Quality Settings
            antiAliasing = serializedObject.FindProperty(LiteRPAssetProperty.AntiAliasing);
            
            // Shadow Settings
            mainLightShadowEnabled = serializedObject.FindProperty(LiteRPAssetProperty.MainLightShadowEnabled);
            mainLightShadowmapResolution = serializedObject.FindProperty(LiteRPAssetProperty.MainLightShadowmapResolution);
            mainLightShadowDistance = serializedObject.FindProperty(LiteRPAssetProperty.MainLightShadowDistance);
            mainLightShadowCascadesCount = serializedObject.FindProperty(LiteRPAssetProperty.MainLightShadowCascadesCount);
            mainLightShadowCascade2Split = serializedObject.FindProperty(LiteRPAssetProperty.MainLightShadowCascades2Split);
            mainLightShadowCascade3Split = serializedObject.FindProperty(LiteRPAssetProperty.MainLightShadowCascades3Split);
            mainLightShadowCascade4Split = serializedObject.FindProperty(LiteRPAssetProperty.MainLightShadowCascades4Split);
            mainLightShadowCascadeBorder = serializedObject.FindProperty(LiteRPAssetProperty.MainLightShadowCascadesBorder);
            mainLightShadowDepthBias = serializedObject.FindProperty(LiteRPAssetProperty.MainLightShadowDepthBias);
            mainLightShadowNormalBias = serializedObject.FindProperty(LiteRPAssetProperty.MainLightShadowNormalBias);
            
            supportsSoftShadows = serializedObject.FindProperty(LiteRPAssetProperty.SupportsSoftShadows);
            softShadowQuality = serializedObject.FindProperty(LiteRPAssetProperty.SoftShadowQuality);
            
            
            // Other Settings
            string Key = "ShadowSettings_Unit:UI_State";
            state = new EditorPrefBoolFlags<EditorUtils.Unit>(Key);
            
            volumeFrameworkUpdateModeProp = serializedObject.FindProperty(LiteRPAssetProperty.VolumeFrameworkUpdateMode);
            volumeProfileProp = serializedObject.FindProperty(LiteRPAssetProperty.VolumeProfile);
        }
        
        public void Update()
        {
            serializedObject.Update();
        }
        
        public void Apply()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}