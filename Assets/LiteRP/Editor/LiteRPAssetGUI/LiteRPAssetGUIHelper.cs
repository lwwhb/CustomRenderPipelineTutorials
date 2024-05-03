using System;
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
            QualitySettings = 1 << 2,
            ShadowSettings = 1 << 3
        }
        internal static class Styles
        {
            // Groups
            public static GUIContent RenderPipelineSettingsText = EditorGUIUtility.TrTextContent("RenderPipelineSettings","渲染管线设置。");
            public static GUIContent QualitySettingsText = EditorGUIUtility.TrTextContent("QualitySettings","图形质量设置。");
            public static GUIContent ShadowSettingsText = EditorGUIUtility.TrTextContent("ShadowSettings","阴影设置。");
            // RenderPipelineSettings
            public static GUIContent SrpBatcherText = EditorGUIUtility.TrTextContent("SRP Batcher", "渲染管线的SRPBatcher功能是否开启。");
            public static GUIContent GpuResidentDrawerModeText = EditorGUIUtility.TrTextContent("GPU Resident Drawer", "通过开启GPU Resident Drawer 来提交绘制以提升CPU性能。");
            public static GUIContent SmallMeshScreenPercentageText = EditorGUIUtility.TrTextContent("Small-Mesh Screen-Percentage", "GPU Driven的Renderers在被剔除前可以覆盖的默认最小屏幕百分比(0-20%)，如果Renders是LODGroup的一部分，此选项将被忽略。");
            public static GUIContent GpuResidentDrawerEnableOcclusionCullingInCamerasText = EditorGUIUtility.TrTextContent("GPU Occlusion Culling", "为GameView和SceneView下的相机启动GPU遮挡剔除");
            
            // ShadowSettings
            public static GUIContent MainLightShadowEnabledText = EditorGUIUtility.TrTextContent("Main Light Shadow", "主光源阴影是否开启。");
            public static GUIContent MainLightShadowmapResolutionText = EditorGUIUtility.TrTextContent("Main Light ShadowMap Resolution", "主光源阴影纹理分辨率。");
            public static GUIContent MainLightShadowDistanceText = EditorGUIUtility.TrTextContent("Main Light Shadow Distance", "主光源阴影范围。");
            public static GUIContent ShadowWorkingUnitText = EditorGUIUtility.TrTextContent("Working Unit", "The unit in which Unity measures the shadow cascade distances. The exception is Max Distance, which will still be in meters.");
            public static GUIContent MainLightShadowCascadesText = EditorGUIUtility.TrTextContent("Cascade Count", "主光源级联阴影划分层级数");
            public static GUIContent MainLightShadowDepthBiasText = EditorGUIUtility.TrTextContent("Depth Bias", "Controls the distance at which the shadows will be pushed away from the light. Useful for avoiding false self-shadowing artifacts.");
            public static GUIContent MainLightShadowNormalBiasText = EditorGUIUtility.TrTextContent("Normal Bias", "Controls distance at which the shadow casting surfaces will be shrunk along the surface normal. Useful for avoiding false self-shadowing artifacts.");
            public static GUIContent SupportsSoftShadowsText = EditorGUIUtility.TrTextContent("Soft Shadows", "If enabled pipeline will perform shadow filtering. Otherwise all lights that cast shadows will fallback to perform a single shadow sample.");
            public static GUIContent SoftShadowsQualityText = EditorGUIUtility.TrTextContent("Quality", "Default shadow quality setting for Lights.");
            public static GUIContent[] SoftShadowsQualityAssetOptions =
            {
                EditorGUIUtility.TrTextContent(nameof(SoftShadowQuality.Low)),
                EditorGUIUtility.TrTextContent(nameof(SoftShadowQuality.Medium)),
                EditorGUIUtility.TrTextContent(nameof(SoftShadowQuality.High))
            };
            public static int[] SoftShadowsQualityAssetValues =  { (int)SoftShadowQuality.Low, (int)SoftShadowQuality.Medium, (int)SoftShadowQuality.High };
            
            // Error Message
            public static GUIContent brgShaderStrippingErrorMessage =
                EditorGUIUtility.TrTextContent("\"BatchRendererGroup Variants\" setting must be \"Keep All\". To fix, modify Graphics settings and set \"BatchRendererGroup Variants\" to \"Keep All\".");
            public static GUIContent staticBatchingInfoMessage =
                EditorGUIUtility.TrTextContent("Static Batching is not recommended when using GPU draw submission modes, performance may improve if Static Batching is disabled in Player Settings.");
            
            public static GUIContent shadowSupportInfoMessage =
                EditorGUIUtility.TrTextContent("SystemInfo.supportsShadows is false, 你的系统环境可能不支持开启阴影渲染。");
        }   
        
        static readonly ExpandedState<Expandable, LiteRPAsset> k_ExpandedState = new(Expandable.RenderPipelineSettings, "LiteRP");
        public static readonly CED.IDrawer Inspector = CED.Group(
            CED.FoldoutGroup(Styles.RenderPipelineSettingsText, Expandable.RenderPipelineSettings, k_ExpandedState, DrawRenderPipelineSettings),
            CED.FoldoutGroup(Styles.QualitySettingsText, Expandable.QualitySettings, k_ExpandedState, DrawQualitySettings),
            CED.FoldoutGroup(Styles.ShadowSettingsText, Expandable.ShadowSettings, k_ExpandedState, DrawShadowSettings)
        );
        
        static void DrawRenderPipelineSettings(SerializedLiteRPAssetProperties serialized, UnityEditor.Editor ownerEditor)
        {
            if (ownerEditor is LiteRPAssetEditor liteRPAssetEditor)
            {
                EditorGUILayout.Space();
                
                EditorGUILayout.PropertyField(serialized.srpBatcher, Styles.SrpBatcherText);

                EditorGUILayout.PropertyField(serialized.gpuResidentDrawerMode, Styles.GpuResidentDrawerModeText);

                var brgStrippingError = EditorGraphicsSettings.batchRendererGroupShaderStrippingMode != BatchRendererGroupStrippingMode.KeepAll;
                var staticBatchingWarning = PlayerSettings.GetStaticBatchingForPlatform(EditorUserBuildSettings.activeBuildTarget);

                if ((GPUResidentDrawerMode)serialized.gpuResidentDrawerMode.intValue != GPUResidentDrawerMode.Disabled)
                {
                    ++EditorGUI.indentLevel;
                    serialized.smallMeshScreenPercentage.floatValue = Mathf.Clamp(EditorGUILayout.FloatField(Styles.SmallMeshScreenPercentageText, serialized.smallMeshScreenPercentage.floatValue), 0.0f, 20.0f);
                    EditorGUILayout.PropertyField(serialized.gpuResidentDrawerEnableOcclusionCullingInCameras, Styles.GpuResidentDrawerEnableOcclusionCullingInCamerasText);
                    --EditorGUI.indentLevel;

                    if (brgStrippingError)
                        EditorGUILayout.HelpBox(Styles.brgShaderStrippingErrorMessage.text, MessageType.Warning, true);
                    if (staticBatchingWarning)
                        EditorGUILayout.HelpBox(Styles.staticBatchingInfoMessage.text, MessageType.Info, true);
                }
            }
        }

        static void DrawQualitySettings(SerializedLiteRPAssetProperties serialized,
            UnityEditor.Editor ownerEditor)
        {
            if (ownerEditor is LiteRPAssetEditor liteRPAssetEditor)
            {
                EditorGUILayout.Space();
            }
        }

        static void DrawShadowSettings(SerializedLiteRPAssetProperties serialized,
            UnityEditor.Editor ownerEditor)
        {
            if (ownerEditor is LiteRPAssetEditor liteRPAssetEditor)
            {
                EditorGUILayout.Space();
                
                bool disableGroup = false;
                disableGroup |= !SystemInfo.supportsShadows;
                if (disableGroup)
                    EditorGUILayout.HelpBox(Styles.shadowSupportInfoMessage.text, MessageType.Warning, true);

                EditorGUI.BeginDisabledGroup(disableGroup);
                EditorGUILayout.PropertyField(serialized.mainLightShadowEnabled, Styles.MainLightShadowEnabledText);
                if (serialized.mainLightShadowEnabled.boolValue)
                {
                    EditorGUILayout.PropertyField(serialized.mainLightShadowmapResolution,
                        Styles.MainLightShadowmapResolutionText);
                    EditorGUILayout.PropertyField(serialized.mainLightShadowDistance,
                        Styles.MainLightShadowDistanceText);
                    EditorUtils.Unit unit = EditorUtils.Unit.Metric;
                    int cascadeCount = serialized.mainLightShadowCascadesCount.intValue;
                    if (cascadeCount != 0)
                    {
                        EditorGUI.BeginChangeCheck();
                        unit = (EditorUtils.Unit)EditorGUILayout.EnumPopup(Styles.ShadowWorkingUnitText, serialized.state.value);
                        if (EditorGUI.EndChangeCheck())
                        {
                            serialized.state.value = unit;
                        }
                    }

                    EditorGUILayout.IntSlider(serialized.mainLightShadowCascadesCount,
                        LiteRPAsset.k_ShadowCascadeMinCount, LiteRPAsset.k_ShadowCascadeMaxCount,
                        Styles.MainLightShadowCascadesText);
                    EditorGUI.indentLevel++;

                    bool useMetric = unit == EditorUtils.Unit.Metric;
                    float baseMetric = serialized.mainLightShadowDistance.floatValue;
                    int cascadeSplitCount = cascadeCount - 1;

                    DrawCascadeSliders(serialized, cascadeSplitCount, useMetric, baseMetric);

                    EditorGUI.indentLevel--;
                    DrawCascades(serialized, cascadeCount, useMetric, baseMetric);
                    
                    serialized.mainLightShadowDepthBias.floatValue = EditorGUILayout.Slider(Styles.MainLightShadowDepthBiasText, serialized.mainLightShadowDepthBias.floatValue, 0.0f, LiteRPAsset.k_MaxShadowBias);
                    serialized.mainLightShadowNormalBias.floatValue = EditorGUILayout.Slider(Styles.MainLightShadowNormalBiasText, serialized.mainLightShadowNormalBias.floatValue, 0.0f, LiteRPAsset.k_MaxShadowBias);
                }
                EditorGUILayout.PropertyField(serialized.supportsSoftShadows, Styles.SupportsSoftShadowsText);
                if (serialized.supportsSoftShadows.boolValue)
                {
                    EditorGUI.indentLevel++;
                    DrawShadowsSoftShadowQuality(serialized, ownerEditor);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.EndDisabledGroup();
            }
        }
        static void DrawCascadeSliders(SerializedLiteRPAssetProperties serialized, int splitCount, bool useMetric, float baseMetric)
        {
            Vector4 shadowCascadeSplit = Vector4.one;
            if (splitCount == 3)
                shadowCascadeSplit = new Vector4(serialized.mainLightShadowCascade4Split.vector3Value.x, serialized.mainLightShadowCascade4Split.vector3Value.y, serialized.mainLightShadowCascade4Split.vector3Value.z, 1);
            else if (splitCount == 2)
                shadowCascadeSplit = new Vector4(serialized.mainLightShadowCascade3Split.vector2Value.x, serialized.mainLightShadowCascade3Split.vector2Value.y, 1, 0);
            else if (splitCount == 1)
                shadowCascadeSplit = new Vector4(serialized.mainLightShadowCascade2Split.floatValue, 1, 0, 0);

            float splitBias = 0.001f;
            float invBaseMetric = baseMetric == 0 ? 0 : 1f / baseMetric;

            // Ensure correct split order
            shadowCascadeSplit[0] = Mathf.Clamp(shadowCascadeSplit[0], 0f, shadowCascadeSplit[1] - splitBias);
            shadowCascadeSplit[1] = Mathf.Clamp(shadowCascadeSplit[1], shadowCascadeSplit[0] + splitBias, shadowCascadeSplit[2] - splitBias);
            shadowCascadeSplit[2] = Mathf.Clamp(shadowCascadeSplit[2], shadowCascadeSplit[1] + splitBias, shadowCascadeSplit[3] - splitBias);


            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < splitCount; ++i)
            {
                float value = shadowCascadeSplit[i];

                float minimum = i == 0 ? 0 : shadowCascadeSplit[i - 1] + splitBias;
                float maximum = i == splitCount - 1 ? 1 : shadowCascadeSplit[i + 1] - splitBias;

                if (useMetric)
                {
                    float valueMetric = value * baseMetric;
                    valueMetric = EditorGUILayout.Slider(EditorGUIUtility.TrTextContent($"Split {i + 1}", "The distance where this cascade ends and the next one starts."), valueMetric, 0f, baseMetric, null);

                    shadowCascadeSplit[i] = Mathf.Clamp(valueMetric * invBaseMetric, minimum, maximum);
                }
                else
                {
                    float valueProcentage = value * 100f;
                    valueProcentage = EditorGUILayout.Slider(EditorGUIUtility.TrTextContent($"Split {i + 1}", "The distance where this cascade ends and the next one starts."), valueProcentage, 0f, 100f, null);

                    shadowCascadeSplit[i] = Mathf.Clamp(valueProcentage * 0.01f, minimum, maximum);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                switch (splitCount)
                {
                    case 3:
                        serialized.mainLightShadowCascade4Split.vector3Value = shadowCascadeSplit;
                        break;
                    case 2:
                        serialized.mainLightShadowCascade3Split.vector2Value = shadowCascadeSplit;
                        break;
                    case 1:
                        serialized.mainLightShadowCascade2Split.floatValue = shadowCascadeSplit.x;
                        break;
                }
            }

            var borderValue = serialized.mainLightShadowCascadeBorder.floatValue;

            EditorGUI.BeginChangeCheck();
            if (useMetric)
            {
                var lastCascadeSplitSize = splitCount == 0 ? baseMetric : (1.0f - shadowCascadeSplit[splitCount - 1]) * baseMetric;
                var invLastCascadeSplitSize = lastCascadeSplitSize == 0 ? 0 : 1f / lastCascadeSplitSize;
                float valueMetric = borderValue * lastCascadeSplitSize;
                valueMetric = EditorGUILayout.Slider(EditorGUIUtility.TrTextContent("Last Border", "The distance of the last cascade."), valueMetric, 0f, lastCascadeSplitSize, null);

                borderValue = valueMetric * invLastCascadeSplitSize;
            }
            else
            {
                float valueProcentage = borderValue * 100f;
                valueProcentage = EditorGUILayout.Slider(EditorGUIUtility.TrTextContent("Last Border", "The distance of the last cascade."), valueProcentage, 0f, 100f, null);

                borderValue = valueProcentage * 0.01f;
            }

            if (EditorGUI.EndChangeCheck())
            {
                serialized.mainLightShadowCascadeBorder.floatValue = borderValue;
            }
        }

        static void DrawCascades(SerializedLiteRPAssetProperties serialized, int cascadeCount, bool useMetric, float baseMetric)
        {
            var cascades = new ShadowCascadeGUI.Cascade[cascadeCount];

            Vector3 shadowCascadeSplit = Vector3.zero;
            if (cascadeCount == 4)
                shadowCascadeSplit = serialized.mainLightShadowCascade4Split.vector3Value;
            else if (cascadeCount == 3)
                shadowCascadeSplit = serialized.mainLightShadowCascade3Split.vector2Value;
            else if (cascadeCount == 2)
                shadowCascadeSplit.x = serialized.mainLightShadowCascade2Split.floatValue;
            else
                shadowCascadeSplit.x = serialized.mainLightShadowCascade2Split.floatValue;

            float lastCascadePartitionSplit = 0;
            for (int i = 0; i < cascadeCount - 1; ++i)
            {
                cascades[i] = new ShadowCascadeGUI.Cascade()
                {
                    size = i == 0 ? shadowCascadeSplit[i] : shadowCascadeSplit[i] - lastCascadePartitionSplit, // Calculate the size of cascade
                    borderSize = 0,
                    cascadeHandleState = ShadowCascadeGUI.HandleState.Enabled,
                    borderHandleState = ShadowCascadeGUI.HandleState.Hidden,
                };
                lastCascadePartitionSplit = shadowCascadeSplit[i];
            }

            // Last cascade is special
            var lastCascade = cascadeCount - 1;
            cascades[lastCascade] = new ShadowCascadeGUI.Cascade()
            {
                size = lastCascade == 0 ? 1.0f : 1 - shadowCascadeSplit[lastCascade - 1], // Calculate the size of cascade
                borderSize = serialized.mainLightShadowCascadeBorder.floatValue,
                cascadeHandleState = ShadowCascadeGUI.HandleState.Hidden,
                borderHandleState = ShadowCascadeGUI.HandleState.Enabled,
            };

            EditorGUI.BeginChangeCheck();
            ShadowCascadeGUI.DrawCascades(ref cascades, useMetric, baseMetric);
            if (EditorGUI.EndChangeCheck())
            {
                if (cascadeCount == 4)
                    serialized.mainLightShadowCascade4Split.vector3Value = new Vector3(
                        cascades[0].size,
                        cascades[0].size + cascades[1].size,
                        cascades[0].size + cascades[1].size + cascades[2].size
                    );
                else if (cascadeCount == 3)
                    serialized.mainLightShadowCascade3Split.vector2Value = new Vector2(
                        cascades[0].size,
                        cascades[0].size + cascades[1].size
                    );
                else if (cascadeCount == 2)
                    serialized.mainLightShadowCascade2Split.floatValue = cascades[0].size;

                serialized.mainLightShadowCascadeBorder.floatValue = cascades[lastCascade].borderSize;
            }
        }
        
        static void DrawShadowsSoftShadowQuality(SerializedLiteRPAssetProperties serialized, UnityEditor.Editor ownerEditor)
        {
            int selectedAssetSoftShadowQuality = serialized.softShadowQuality.intValue;
            Rect r = EditorGUILayout.GetControlRect(true);
            EditorGUI.BeginProperty(r, Styles.SoftShadowsQualityText, serialized.softShadowQuality);
            {
                using (var checkScope = new EditorGUI.ChangeCheckScope())
                {
                    selectedAssetSoftShadowQuality = EditorGUI.IntPopup(r, Styles.SoftShadowsQualityText, selectedAssetSoftShadowQuality, Styles.SoftShadowsQualityAssetOptions, Styles.SoftShadowsQualityAssetValues);
                    if (checkScope.changed)
                    {
                        serialized.softShadowQuality.intValue = Math.Clamp(selectedAssetSoftShadowQuality, (int)SoftShadowQuality.Low, (int)SoftShadowQuality.High);
                    }
                }
            }
            EditorGUI.EndProperty();
        }

    }
}