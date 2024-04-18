using UnityEditor;

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