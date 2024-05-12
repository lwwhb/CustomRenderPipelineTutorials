using LiteRP.AdditionalData;
using UnityEditor;
using UnityEditor.Rendering;

namespace LiteRP.Editor
{
    public class SerializedLiteRPLightProperties : ISerializedLight
    {
        public LightEditor.Settings settings { get; }
        public SerializedObject serializedObject { get; }
        public SerializedObject serializedAdditionalDataObject { get; }
        
        public AdditionalLightData[] lightsAdditionalData { get; private set; }
        public AdditionalLightData additionalLightData => lightsAdditionalData[0];
        
        // Common SRP's Lights properties
        public SerializedProperty intensity { get; }
        
        // LiteRP Light Properties
        public SerializedProperty useAdditionalDataProp { get; }                     // 灯光是否使用LiteRP Asset文件中定义的Shadow bias Settings
        public SerializedProperty additionalLightsShadowResolutionTierProp { get; }  // AdditionalLights阴影分辨率层级索引
        public SerializedProperty softShadowQualityProp { get; }                     // 软阴影质量
        public SerializedProperty lightCookieSizeProp { get; }                       // 多维灯光Cookie Size
        public SerializedProperty lightCookieOffsetProp { get; }                     // 多维灯光Cookie Size Offset.
        
        // Light layers related
        public SerializedProperty renderingLayers { get; }
        public SerializedProperty customShadowLayers { get; }
        public SerializedProperty shadowRenderingLayers { get; }
        public SerializedLiteRPLightProperties(SerializedObject serializedObject, LightEditor.Settings settings)
        {
            this.settings = settings;
            settings.OnEnable();

            this.serializedObject = serializedObject;

            lightsAdditionalData = CoreEditorUtils
                .GetAdditionalData<AdditionalLightData>(serializedObject.targetObjects);
            serializedAdditionalDataObject = new SerializedObject(lightsAdditionalData);

            intensity = serializedObject.FindProperty("m_Intensity");

            useAdditionalDataProp = serializedAdditionalDataObject.FindProperty("m_UsePipelineSettings");
            additionalLightsShadowResolutionTierProp = serializedAdditionalDataObject.FindProperty("m_AdditionalLightsShadowResolutionTier");
            softShadowQualityProp = serializedAdditionalDataObject.FindProperty("m_SoftShadowQuality");
            lightCookieSizeProp = serializedAdditionalDataObject.FindProperty("m_LightCookieSize");
            lightCookieOffsetProp = serializedAdditionalDataObject.FindProperty("m_LightCookieOffset");

            renderingLayers = serializedAdditionalDataObject.FindProperty("m_RenderingLayers");
            customShadowLayers = serializedAdditionalDataObject.FindProperty("m_CustomShadowLayers");
            shadowRenderingLayers = serializedAdditionalDataObject.FindProperty("m_ShadowRenderingLayers");

            settings.ApplyModifiedProperties();
        }
        
        public void Update()
        {
            serializedObject.Update();
            serializedAdditionalDataObject.Update();
            settings.Update();
        }

        public void Apply()
        {
            serializedObject.ApplyModifiedProperties();
            serializedAdditionalDataObject.ApplyModifiedProperties();
            settings.ApplyModifiedProperties();
        }

        
    }
}