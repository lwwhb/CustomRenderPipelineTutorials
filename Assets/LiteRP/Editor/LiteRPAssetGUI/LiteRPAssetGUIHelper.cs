using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Editor
{
    using CED = CoreEditorDrawer<SerializedLiteRPAssetProperties>;
    internal static class LiteRPAssetGUIHelper 
    {
        enum Expandable
        {
            RenderPipelineSettings = 1 << 1,
        }
        internal static class Styles
        {
            // Groups
            public static GUIContent RenderPipelineSettingsText = EditorGUIUtility.TrTextContent("RenderPipelineSettings","渲染管线设置。");
            // RenderPipelineSettings
            public static GUIContent srpBatcher = EditorGUIUtility.TrTextContent("SRP Batcher", "渲染管线的SRPBatcher功能是否开启。");
            public static GUIContent gpuResidentDrawerMode = EditorGUIUtility.TrTextContent("GPU Resident Drawer", "通过开启GPU Resident Drawer 来提交绘制以提升CPU性能。");
            public static GUIContent smallMeshScreenPercentage = EditorGUIUtility.TrTextContent("Small-Mesh Screen-Percentage", "GPU Driven的Renderers在被剔除前可以覆盖的默认最小屏幕百分比(0-20%)，如果Renders是LODGroup的一部分，此选项将被忽略。");
            public static GUIContent gpuResidentDrawerEnableOcclusionCullingInCameras = EditorGUIUtility.TrTextContent("GPU Occlusion Culling", "为GameView和SceneView下的相机启动GPU遮挡剔除");
            
            // Error Message
            public static GUIContent brgShaderStrippingErrorMessage =
                EditorGUIUtility.TrTextContent("\"BatchRendererGroup Variants\" setting must be \"Keep All\". To fix, modify Graphics settings and set \"BatchRendererGroup Variants\" to \"Keep All\".");
            public static GUIContent staticBatchingInfoMessage =
                EditorGUIUtility.TrTextContent("Static Batching is not recommended when using GPU draw submission modes, performance may improve if Static Batching is disabled in Player Settings.");
        }   
        
        static readonly ExpandedState<Expandable, LiteRPAsset> k_ExpandedState = new(Expandable.RenderPipelineSettings, "LiteRP");
        public static readonly CED.IDrawer Inspector = CED.Group(
            CED.FoldoutGroup(Styles.RenderPipelineSettingsText, Expandable.RenderPipelineSettings, k_ExpandedState, DrawRenderPipelineSettings)
        );
        
        static void DrawRenderPipelineSettings(SerializedLiteRPAssetProperties serialized, UnityEditor.Editor ownerEditor)
        {
            if (ownerEditor is LiteRPAssetEditor liteRPAssetEditor)
            {
                EditorGUILayout.Space();
                
                EditorGUILayout.PropertyField(serialized.srpBatcher, Styles.srpBatcher);

                EditorGUILayout.PropertyField(serialized.gpuResidentDrawerMode, Styles.gpuResidentDrawerMode);

                var brgStrippingError = EditorGraphicsSettings.batchRendererGroupShaderStrippingMode != BatchRendererGroupStrippingMode.KeepAll;
                var staticBatchingWarning = PlayerSettings.GetStaticBatchingForPlatform(EditorUserBuildSettings.activeBuildTarget);

                if ((GPUResidentDrawerMode)serialized.gpuResidentDrawerMode.intValue != GPUResidentDrawerMode.Disabled)
                {
                    ++EditorGUI.indentLevel;
                    serialized.smallMeshScreenPercentage.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(Styles.smallMeshScreenPercentage, serialized.smallMeshScreenPercentage.floatValue), 0.0f, 20.0f);
                    EditorGUILayout.PropertyField(serialized.gpuResidentDrawerEnableOcclusionCullingInCameras, Styles.gpuResidentDrawerEnableOcclusionCullingInCameras);
                    --EditorGUI.indentLevel;

                    if (brgStrippingError)
                        EditorGUILayout.HelpBox(Styles.brgShaderStrippingErrorMessage.text, MessageType.Warning, true);
                    if (staticBatchingWarning)
                        EditorGUILayout.HelpBox(Styles.staticBatchingInfoMessage.text, MessageType.Info, true);
                }
            }
        }
    }
}