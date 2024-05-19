using LiteRP.AdditionalData;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace LiteRP.Editor
{
    using CED = CoreEditorDrawer<SerializedLiteRPCameraProperties>;
    public partial class PhysicalCamera{
        public static readonly CED.IDrawer Drawer;
        static PhysicalCamera()
        {
            Drawer = CED.Conditional(
                (serialized, owner) => serialized.projectionMatrixMode.intValue == (int)CameraUI.ProjectionMatrixMode.PhysicalPropertiesBased,
                CED.Group(
                    CameraUI.PhysicalCamera.Styles.cameraBody,
                    GroupOption.Indent,
                    CED.Group(
                        GroupOption.Indent,
                        CameraUI.PhysicalCamera.Drawer_PhysicalCamera_CameraBody_Sensor,
                        CameraUI.PhysicalCamera.Drawer_PhysicalCamera_CameraBody_ISO,
                        CameraUI.PhysicalCamera.Drawer_PhysicalCamera_CameraBody_ShutterSpeed,
                        CameraUI.PhysicalCamera.Drawer_PhysicalCamera_CameraBody_GateFit
                    )
                ),
                CED.Group(
                    CameraUI.PhysicalCamera.Styles.lens,
                    GroupOption.Indent,
                    CED.Group(
                        GroupOption.Indent,
                        CameraUI.PhysicalCamera.Drawer_PhysicalCamera_Lens_FocalLength,
                        CameraUI.PhysicalCamera.Drawer_PhysicalCamera_Lens_Shift,
                        CameraUI.PhysicalCamera.Drawer_PhysicalCamera_Lens_Aperture,
                        CameraUI.PhysicalCamera.Drawer_PhysicalCamera_FocusDistance
                    )
                ),
                CED.Group(
                    CameraUI.PhysicalCamera.Styles.apertureShape,
                    GroupOption.Indent,
                    CED.Group(
                        GroupOption.Indent,
                        CameraUI.PhysicalCamera.Drawer_PhysicalCamera_ApertureShape
                    )
                )
            );
        }
    }
    internal enum BackgroundType
    {
        Skybox = 0,
        SolidColor,
        [InspectorName("Uninitialized")]
        DontCare,
    }
    internal static class LiteRPCameraGUIHelper
    {
        public enum Expandable
        {
            /// <summary> Projection</summary>
            Projection = 1 << 0,
            /// <summary> Physical</summary>
            Physical = 1 << 1,
            /// <summary> Output</summary>
            Output = 1 << 2,
            /// <summary> Orthographic</summary>
            Orthographic = 1 << 3,
            /// <summary> RenderLoop</summary>
            RenderLoop = 1 << 4,
            /// <summary> Rendering</summary>
            Rendering = 1 << 5,
            /// <summary> Environment</summary>
            Environment = 1 << 6,
        }
        
        internal static class Styles
        {
            public static GUIContent renderPostProcessing = EditorGUIUtility.TrTextContent("Post Processing", "Enable this to make this camera render post-processing effects.");
            public static GUIContent renderingShadows = EditorGUIUtility.TrTextContent("Render Shadows", "Makes this camera render shadows.");
            public static GUIContent priority = EditorGUIUtility.TrTextContent("Priority", "A camera with a higher priority is drawn on top of a camera with a lower priority [ -100, 100 ].");
            
            public static GUIContent backgroundType = EditorGUIUtility.TrTextContent("Background Type", "Controls how to initialize the Camera's background.\n\nSkybox initializes camera with Skybox, defaulting to a background color if no skybox is found.\n\nSolid Color initializes background with the background color.\n\nUninitialized has undefined values for the camera background. Use this only if you are rendering all pixels in the Camera's view.");
            public static GUIContent volumesSettingsText = EditorGUIUtility.TrTextContent("Volumes", "These settings define how Volumes affect this Camera.");
            public static GUIContent volumeTrigger = EditorGUIUtility.TrTextContent("Volume Trigger", "A transform that will act as a trigger for volume blending. If none is set, the camera itself will act as a trigger.");
            public static GUIContent volumeUpdates = EditorGUIUtility.TrTextContent("Update Mode", "Select how Unity updates Volumes: every frame or when triggered via scripting. In the Editor, Unity updates Volumes every frame when not in the Play mode.");
            
            public static readonly GUIContent targetTextureLabel = EditorGUIUtility.TrTextContent("Output Texture", "The texture to render this camera into, if none then this camera renders to screen.");
            public static readonly GUIContent allowMSAA = EditorGUIUtility.TrTextContent("MSAA", "Enables Multi-Sample Anti-Aliasing, a technique that smooths jagged edges.");
            public static readonly GUIContent allowHDR = EditorGUIUtility.TrTextContent("HDR Rendering", "High Dynamic Range gives you a wider range of light intensities, so your lighting looks more realistic. With it, you can still see details and experience less saturation even with bright light.", (Texture)null);
            public static readonly GUIContent allowDynamicResolution = EditorGUIUtility.TrTextContent("URP Dynamic Resolution", "Whether to support URP dynamic resolution.");
            public static readonly GUIContent allowHDROutput = EditorGUIUtility.TrTextContent("HDR Output", "Whether to support outputting to HDR displays.");

            public static string cameraTargetTextureMSAA = L10n.Tr("Camera target texture requires {0}x MSAA. Universal pipeline {1}.");
            public static string pipelineMSAACapsSupportSamples = L10n.Tr("is set to support {0}x");
            public static string pipelineMSAACapsDisabled = L10n.Tr("has MSAA disabled");
            public static string disabledHDRRenderingWithHDROutput = L10n.Tr("HDR Output is enabled but HDR rendering is disabled. Image may appear underexposed or oversaturated on an HDR display.");
            
            public static GUIContent[] displayedCameraOptions =
            {
                EditorGUIUtility.TrTextContent("Off"),
                EditorGUIUtility.TrTextContent("Use settings from Render Pipeline Asset"),
            };
            public static int[] cameraOptions = { 0, 1 };
            
            public static GUIContent[] hdrOuputOptions =
            {
                EditorGUIUtility.TrTextContent("Off"),
                EditorGUIUtility.TrTextContent("Use Project Settings"),
            };
            public static int[] hdrOuputValues = { 0, 1 };
        }
        static readonly ExpandedState<Expandable, Camera> k_ExpandedState = new(Expandable.Projection, "LiteRP");
        public static readonly CED.IDrawer Inspector = CED.Group(
            CED.FoldoutGroup(CameraUI.Styles.projectionSettingsHeaderContent,
                Expandable.Projection,
                k_ExpandedState,
                FoldoutOption.Indent,
                CED.Group(
                    DrawProjectionSettings
                ),
                PhysicalCamera.Drawer),
            CED.FoldoutGroup(CameraUI.Rendering.Styles.header,
                Expandable.Rendering,
                k_ExpandedState,
                FoldoutOption.Indent,
                DrawRenderingSettings),
            CED.FoldoutGroup(CameraUI.Environment.Styles.header,
                Expandable.Environment,
                k_ExpandedState,
                FoldoutOption.Indent,
                CED.Group(
                    Drawer_Environment_ClearFlags
                ),
                CED.Group(
                    Styles.volumesSettingsText,
                    CED.Group(
                        GroupOption.Indent,
                        Drawer_Environment_VolumeUpdate,
                        CameraUI.Environment.Drawer_Environment_VolumeLayerMask,
                        Drawer_Environment_VolumeTrigger
                    )
                )),
            CED.FoldoutGroup(CameraUI.Output.Styles.header,
                Expandable.Output,
                k_ExpandedState,
                FoldoutOption.Indent,
                CED.Group(
                    DrawerOutputTargetTexture
                ),
                CED.Conditional(
                    (serialized, owner) => serialized.serializedObject.targetObject is Camera camera && camera.targetTexture == null,
                    CED.Group(
                        DrawerOutputMultiDisplay
                    )
                ),
                CED.Group(
                    DrawerOutputNormalizedViewPort
                ),
                CED.Conditional(
                    (serialized, owner) => serialized.serializedObject.targetObject is Camera camera && camera.targetTexture == null,
                    CED.Group(
                        CED.Group(DrawerOutputHDR),
                        CED.Conditional(
                            (serialized, owner) => PlayerSettings.allowHDRDisplaySupport,
                            CED.Group(DrawerOutputHDROutput)
                        ),
                        CED.Group(DrawerOutputMSAA),
                        CED.Group(DrawerOutputAllowDynamicResolution)
                    )
                ))
            );
        static void DrawProjectionSettings(SerializedLiteRPCameraProperties p, UnityEditor.Editor owner)
        {
            CameraUI.Drawer_Projection(p, owner);
        }
        static void DrawRenderingSettings(SerializedLiteRPCameraProperties p, UnityEditor.Editor owner)
        {
            EditorGUILayout.PropertyField(p.renderPostProcessing, Styles.renderPostProcessing);
            CameraUI.Rendering.Drawer_Rendering_StopNaNs(p, owner);
            CameraUI.Rendering.Drawer_Rendering_Dithering(p, owner);
            EditorGUILayout.PropertyField(p.renderShadows, Styles.renderingShadows);
            EditorGUILayout.PropertyField(p.baseCameraSettings.depth, Styles.priority);
            CameraUI.Rendering.Drawer_Rendering_CullingMask(p, owner);
            CameraUI.Rendering.Drawer_Rendering_OcclusionCulling(p, owner);
        }
        static BackgroundType GetBackgroundType(CameraClearFlags clearFlags)
        {
            switch (clearFlags)
            {
                case CameraClearFlags.Skybox:
                    return BackgroundType.Skybox;
                case CameraClearFlags.Nothing:
                    return BackgroundType.DontCare;

                // DepthOnly is not supported by design in LiteRP. We upgrade it to SolidColor
                default:
                    return BackgroundType.SolidColor;
            }
        }
        static void Drawer_Environment_ClearFlags(SerializedLiteRPCameraProperties p, UnityEditor.Editor owner)
        {
            EditorGUI.showMixedValue = p.baseCameraSettings.clearFlags.hasMultipleDifferentValues;

            Rect clearFlagsRect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginProperty(clearFlagsRect, Styles.backgroundType, p.baseCameraSettings.clearFlags);
            {
                EditorGUI.BeginChangeCheck();
                BackgroundType backgroundType = GetBackgroundType((CameraClearFlags)p.baseCameraSettings.clearFlags.intValue);
                var selectedValue = (BackgroundType)EditorGUI.EnumPopup(clearFlagsRect, Styles.backgroundType, backgroundType);
                if (EditorGUI.EndChangeCheck())
                {
                    CameraClearFlags selectedClearFlags;
                    switch (selectedValue)
                    {
                        case BackgroundType.Skybox:
                            selectedClearFlags = CameraClearFlags.Skybox;
                            break;

                        case BackgroundType.DontCare:
                            selectedClearFlags = CameraClearFlags.Nothing;
                            break;

                        default:
                            selectedClearFlags = CameraClearFlags.SolidColor;
                            break;
                    }

                    p.baseCameraSettings.clearFlags.intValue = (int)selectedClearFlags;
                }

                if (!p.baseCameraSettings.clearFlags.hasMultipleDifferentValues)
                {
                    if (GetBackgroundType((CameraClearFlags)p.baseCameraSettings.clearFlags.intValue) == BackgroundType.SolidColor)
                    {
                        using (var group = new EditorGUI.IndentLevelScope())
                        {
                            p.baseCameraSettings.DrawBackgroundColor();
                        }
                    }
                }
            }
            EditorGUI.EndProperty();
            EditorGUI.showMixedValue = false;
        }
        static void Drawer_Environment_VolumeUpdate(SerializedLiteRPCameraProperties p, UnityEditor.Editor owner)
        {
            Rect volumeUpdateRect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginChangeCheck();
            VolumeFrameworkUpdateMode prevVolumeUpdateMode = (VolumeFrameworkUpdateMode)p.volumeFrameworkUpdateMode.intValue;
            VolumeFrameworkUpdateMode selectedValue = (VolumeFrameworkUpdateMode)EditorGUI.EnumPopup(volumeUpdateRect, Styles.volumeUpdates, prevVolumeUpdateMode);
            if (EditorGUI.EndChangeCheck())
            {
                if (p.serializedObject.targetObject is not Camera cam)
                    return;

                VolumeFrameworkUpdateMode curVolumeUpdateMode = (VolumeFrameworkUpdateMode)p.volumeFrameworkUpdateMode.intValue;
                cam.SetVolumeFrameworkUpdateMode(curVolumeUpdateMode);
                p.volumeFrameworkUpdateMode.intValue = (int)selectedValue;
            }
        }

        static void Drawer_Environment_VolumeTrigger(SerializedLiteRPCameraProperties p, UnityEditor.Editor owner)
        {
            var controlRect = EditorGUILayout.GetControlRect(true);
            EditorGUI.BeginProperty(controlRect, Styles.volumeTrigger, p.volumeTrigger);
            {
                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUI.ObjectField(controlRect, Styles.volumeTrigger, (Transform)p.volumeTrigger.objectReferenceValue, typeof(Transform), true);
                if (EditorGUI.EndChangeCheck() && !Equals(p.volumeTrigger.objectReferenceValue, newValue))
                    p.volumeTrigger.objectReferenceValue = newValue;
            }
            EditorGUI.EndProperty();
        }
        static void DrawerOutputTargetTexture(SerializedLiteRPCameraProperties p, UnityEditor.Editor owner)
        {
            var rpAsset = LiteRenderPipeline.asset;
            using (var checkScope = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(p.baseCameraSettings.targetTexture, Styles.targetTextureLabel);

                var texture = p.baseCameraSettings.targetTexture.objectReferenceValue as RenderTexture;
                if (!p.baseCameraSettings.targetTexture.hasMultipleDifferentValues && rpAsset != null)
                {
                    int pipelineSamplesCount = rpAsset.msaaSampleCount;

                    if (texture && texture.antiAliasing > pipelineSamplesCount)
                    {
                        string pipelineMSAACaps = (pipelineSamplesCount > 1) ? string.Format(Styles.pipelineMSAACapsSupportSamples, pipelineSamplesCount) : Styles.pipelineMSAACapsDisabled;
                        EditorGUILayout.HelpBox(string.Format(Styles.cameraTargetTextureMSAA, texture.antiAliasing, pipelineMSAACaps), MessageType.Warning, true);
                    }
                }
                
                if (checkScope.changed)
                {
                    var camera = p.serializedObject.targetObject as Camera;
                    if (camera != null && camera.targetTexture != texture)
                        camera.targetTexture = texture;
                }
            }
        }
        
        static void DrawerOutputMultiDisplay(SerializedLiteRPCameraProperties p, UnityEditor.Editor owner)
        {
            using (var checkScope = new EditorGUI.ChangeCheckScope())
            {
                p.baseCameraSettings.DrawMultiDisplay();
                if (checkScope.changed)
                {
                    var camera = p.serializedObject.targetObject as Camera;
                    if (camera != null)
                    {
                        // Force same target display
                        int targetDisplay = p.baseCameraSettings.targetDisplay.intValue;
                        if (camera.targetDisplay != targetDisplay)
                            camera.targetDisplay = targetDisplay;
                    }
                }
            }
        }
        
        static void DrawerOutputNormalizedViewPort(SerializedLiteRPCameraProperties p, UnityEditor.Editor owner)
        {
            using (var checkScope = new EditorGUI.ChangeCheckScope())
            {
                CameraUI.Output.Drawer_Output_NormalizedViewPort(p, owner);
                if (checkScope.changed)
                {
                    var camera = p.serializedObject.targetObject as Camera;
                    if (camera != null)
                    {
                        Rect rect = p.baseCameraSettings.normalizedViewPortRect.rectValue;
                        if (camera.rect != rect)
                            camera.rect = p.baseCameraSettings.normalizedViewPortRect.rectValue;
                    }
                }
            }
        }
        
        static void DrawerOutputHDR(SerializedLiteRPCameraProperties p, UnityEditor.Editor owner)
        {
            Rect controlRect = EditorGUILayout.GetControlRect(true);
            EditorGUI.BeginProperty(controlRect, Styles.allowHDR, p.baseCameraSettings.HDR);
            {
                using (var checkScope = new EditorGUI.ChangeCheckScope())
                {
                    int selectedValue = !p.baseCameraSettings.HDR.boolValue ? 0 : 1;
                    var allowHDR = EditorGUI.IntPopup(controlRect, Styles.allowHDR, selectedValue, Styles.displayedCameraOptions, Styles.cameraOptions) == 1;
                    if (checkScope.changed)
                    {
                        var camera = p.serializedObject.targetObject as Camera;
                        if (camera != null)
                        {
                            p.baseCameraSettings.HDR.boolValue = allowHDR;
                            if (camera.allowHDR != allowHDR)
                                camera.allowHDR = allowHDR;
                        }
                    }
                }
            }
            EditorGUI.EndProperty();
        }
        
        static void DrawerOutputHDROutput(SerializedLiteRPCameraProperties p, UnityEditor.Editor owner)
        {
            Rect controlRect = EditorGUILayout.GetControlRect(true);
            EditorGUI.BeginProperty(controlRect, Styles.allowHDROutput, p.allowHDROutput);
            {
                using (var checkScope = new EditorGUI.ChangeCheckScope())
                {
                    int selectedValue = !p.allowHDROutput.boolValue ? 0 : 1;
                    var allowHDROutput = EditorGUI.IntPopup(controlRect, Styles.allowHDROutput, selectedValue, Styles.hdrOuputOptions, Styles.hdrOuputValues) == 1;

                    var rpAsset = LiteRenderPipeline.asset;
                    bool perCameraHDRDisabled = !p.baseCameraSettings.HDR.boolValue && (rpAsset == null || rpAsset.supportsHDR);
                        
                    if (allowHDROutput && PlayerSettings.allowHDRDisplaySupport && perCameraHDRDisabled)
                    {
                        EditorGUILayout.HelpBox(Styles.disabledHDRRenderingWithHDROutput, MessageType.Warning);
                    }

                    if (checkScope.changed)
                        p.allowHDROutput.boolValue = allowHDROutput;
                }
            }
            EditorGUI.EndProperty();
        }

        static void DrawerOutputMSAA(SerializedLiteRPCameraProperties p, UnityEditor.Editor owner)
        {
            Rect controlRect = EditorGUILayout.GetControlRect(true);
            EditorGUI.BeginProperty(controlRect, Styles.allowMSAA, p.baseCameraSettings.allowMSAA);
            {
                using (var checkScope = new EditorGUI.ChangeCheckScope())
                {
                    int selectedValue = !p.baseCameraSettings.allowMSAA.boolValue ? 0 : 1;
                    var allowMSAA = EditorGUI.IntPopup(controlRect, Styles.allowMSAA,
                        selectedValue, Styles.displayedCameraOptions, Styles.cameraOptions) == 1;
                    if (checkScope.changed)
                    {
                        var camera = p.serializedObject.targetObject as Camera;
                        if (camera != null)
                        {
                            p.baseCameraSettings.allowMSAA.boolValue = allowMSAA;
                            if (camera.allowMSAA != allowMSAA)
                                camera.allowMSAA = allowMSAA;
                        }
                    }
                }
            }
            EditorGUI.EndProperty();
        }
        
        static void DrawerOutputAllowDynamicResolution(SerializedLiteRPCameraProperties p, UnityEditor.Editor owner)
        {
            using (var checkScope = new EditorGUI.ChangeCheckScope())
            {
                CameraUI.Output.Drawer_Output_AllowDynamicResolution(p, owner, Styles.allowDynamicResolution);
                if (checkScope.changed)
                {
                    var camera = p.serializedObject.targetObject as Camera;
                    if (camera != null)
                    {
                        bool allowDynamicResolution = p.allowDynamicResolution.boolValue;
                        if (camera.allowDynamicResolution != p.allowDynamicResolution.boolValue)
                        {
                            EditorUtility.SetDirty(camera);
                            camera.allowDynamicResolution = allowDynamicResolution;
                        }
                    }
                }
            }
        }

    }
}