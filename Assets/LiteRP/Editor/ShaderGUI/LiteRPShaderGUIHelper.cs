
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.ShaderGraph.Drawing;
using UnityEngine;
using SurfaceType = LiteRP.Editor.LiteRPShaderGUI.SurfaceType;
using BlendMode = LiteRP.Editor.LiteRPShaderGUI.BlendMode;
using RenderFace = LiteRP.Editor.LiteRPShaderGUI.RenderFace;
using ZWriteControl = LiteRP.Editor.LiteRPShaderGUI.ZWriteControl;
using ZTestMode = LiteRP.Editor.LiteRPShaderGUI.ZTestMode;
using QueueControl = LiteRP.Editor.LiteRPShaderGUI.QueueControl;
namespace LiteRP.Editor
{
    public static class LiteRPShaderGUIHelper 
    {
        internal class Styles
        {
            /// <summary>
            /// The names for options available in the SurfaceType enum.
            /// </summary>
            public static readonly string[] surfaceTypeNames = Enum.GetNames(typeof(SurfaceType));

            /// <summary>
            /// The names for options available in the BlendMode enum.
            /// </summary>
            public static readonly string[] blendModeNames = Enum.GetNames(typeof(BlendMode));

            /// <summary>
            /// The names for options available in the RenderFace enum.
            /// </summary>
            public static readonly string[] renderFaceNames = Enum.GetNames(typeof(RenderFace));

            /// <summary>
            /// The names for options available in the ZWriteControl enum.
            /// </summary>
            public static readonly string[] zwriteNames = Enum.GetNames(typeof(ZWriteControl));

            /// <summary>
            /// The names for options available in the QueueControl enum.
            /// </summary>
            public static readonly string[] queueControlNames = Enum.GetNames(typeof(QueueControl));

            /// <summary>
            /// The values for options available in the ZTestMode enum.
            /// </summary>
            // Skipping the first entry for ztest (ZTestMode.Disabled is not a valid value)
            public static readonly int[] ztestValues = ((int[])Enum.GetValues(typeof(ZTestMode))).Skip(1).ToArray();

            /// <summary>
            /// The names for options available in the ZTestMode enum.
            /// </summary>
            // Skipping the first entry for ztest (ZTestMode.Disabled is not a valid value)
            public static readonly string[] ztestNames = Enum.GetNames(typeof(ZTestMode)).Skip(1).ToArray();

            // Categories
            /// <summary>
            /// The text and tooltip for the surface options GUI.
            /// </summary>
            public static readonly GUIContent SurfaceOptions =
                EditorGUIUtility.TrTextContent("Surface Options", "Controls how URP Renders the material on screen.");

            /// <summary>
            /// The text and tooltip for the surface inputs GUI.
            /// </summary>
            public static readonly GUIContent SurfaceInputs = EditorGUIUtility.TrTextContent("Surface Inputs",
                "These settings describe the look and feel of the surface itself.");

            /// <summary>
            /// The text and tooltip for the advanced options GUI.
            /// </summary>
            public static readonly GUIContent AdvancedLabel = EditorGUIUtility.TrTextContent("Advanced Options",
                "These settings affect behind-the-scenes rendering and underlying calculations.");

            /// <summary>
            /// The text and tooltip for the Surface Type GUI.
            /// </summary>
            public static readonly GUIContent surfaceType = EditorGUIUtility.TrTextContent("Surface Type",
                "Select a surface type for your texture. Choose between Opaque or Transparent.");

            /// <summary>
            /// The text and tooltip for the blending mode GUI.
            /// </summary>
            public static readonly GUIContent blendingMode = EditorGUIUtility.TrTextContent("Blending Mode",
                "Controls how the color of the Transparent surface blends with the Material color in the background.");

            /// <summary>
            /// The text and tooltip for the preserve specular lighting GUI.
            /// </summary>
            public static readonly GUIContent preserveSpecularText = EditorGUIUtility.TrTextContent("Preserve Specular Lighting",
                "Preserves specular lighting intensity and size by not applying transparent alpha to the specular light contribution.");

            /// <summary>
            /// The text and tooltip for the render face GUI.
            /// </summary>
            public static readonly GUIContent cullingText = EditorGUIUtility.TrTextContent("Render Face",
                "Specifies which faces to cull from your geometry. Front culls front faces. Back culls back faces. Both means that both sides are rendered.");

            /// <summary>
            /// The text and tooltip for the depth write GUI.
            /// </summary>
            public static readonly GUIContent zwriteText = EditorGUIUtility.TrTextContent("Depth Write",
                "Controls whether the shader writes depth.  Auto will write only when the shader is opaque.");

            /// <summary>
            /// The text and tooltip for the depth test GUI.
            /// </summary>
            public static readonly GUIContent ztestText = EditorGUIUtility.TrTextContent("Depth Test",
                "Specifies the depth test mode.  The default is LEqual.");

            /// <summary>
            /// The text and tooltip for the alpha clipping GUI.
            /// </summary>
            public static readonly GUIContent alphaClipText = EditorGUIUtility.TrTextContent("Alpha Clipping",
                "Makes your Material act like a Cutout shader. Use this to create a transparent effect with hard edges between opaque and transparent areas. Avoid using when Alpha is constant for the entire material as enabling in this case could introduce visual artifacts and will add an unnecessary performance cost when used with MSAA (due to AlphaToMask).");

            /// <summary>
            /// The text and tooltip for the alpha clipping threshold GUI.
            /// </summary>
            public static readonly GUIContent alphaClipThresholdText = EditorGUIUtility.TrTextContent("Threshold",
                "Sets where the Alpha Clipping starts. The higher the value is, the brighter the  effect is when clipping starts.");

            /// <summary>
            /// The text and tooltip for the cast shadows GUI.
            /// </summary>
            public static readonly GUIContent castShadowText = EditorGUIUtility.TrTextContent("Cast Shadows",
                "When enabled, this GameObject will cast shadows onto any geometry that can receive them.");

            /// <summary>
            /// The text and tooltip for the receive shadows GUI.
            /// </summary>
            public static readonly GUIContent receiveShadowText = EditorGUIUtility.TrTextContent("Receive Shadows",
                "When enabled, other GameObjects can cast shadows onto this GameObject.");

            /// <summary>
            /// The text and tooltip for the base map GUI.
            /// </summary>
            public static readonly GUIContent baseMap = EditorGUIUtility.TrTextContent("Base Map",
                "Specifies the base Material and/or Color of the surface. If you’ve selected Transparent or Alpha Clipping under Surface Options, your Material uses the Texture’s alpha channel or color.");

            /// <summary>
            /// The text and tooltip for the emission map GUI.
            /// </summary>
            public static readonly GUIContent emissionMap = EditorGUIUtility.TrTextContent("Emission Map",
                "Determines the color and intensity of light that the surface of the material emits.");

            /// <summary>
            /// The text and tooltip for the normal map GUI.
            /// </summary>
            public static readonly GUIContent normalMapText =
                EditorGUIUtility.TrTextContent("Normal Map", "Designates a Normal Map to create the illusion of bumps and dents on this Material's surface.");

            /// <summary>
            /// The text and tooltip for the bump scale not supported GUI.
            /// </summary>
            public static readonly GUIContent bumpScaleNotSupported =
                EditorGUIUtility.TrTextContent("Bump scale is not supported on mobile platforms");

            /// <summary>
            /// The text and tooltip for the normals fix now GUI.
            /// </summary>
            public static readonly GUIContent fixNormalNow = EditorGUIUtility.TrTextContent("Fix now",
                "Converts the assigned texture to be a normal map format.");

            /// <summary>
            /// The text and tooltip for the sorting priority GUI.
            /// </summary>
            public static readonly GUIContent queueSlider = EditorGUIUtility.TrTextContent("Sorting Priority",
                "Determines the chronological rendering order for a Material. Materials with lower value are rendered first.");

            /// <summary>
            /// The text and tooltip for the queue control GUI.
            /// </summary>
            public static readonly GUIContent queueControl = EditorGUIUtility.TrTextContent("Queue Control",
                "Controls whether render queue is automatically set based on material surface type, or explicitly set by the user.");

            /// <summary>
            /// The text and tooltip for the help reference GUI.
            /// </summary>
            public static readonly GUIContent documentationIcon = EditorGUIUtility.TrIconContent("_Help", $"Open Reference for URP Shaders.");
        }
        // 绘制Base属性
        internal static void DrawBaseProperties(MaterialEditor materialEditor, MaterialProperty baseMapProperty, MaterialProperty baseColorProperty)
        {
            if (baseMapProperty != null && baseColorProperty != null) 
                materialEditor.TexturePropertySingleLine(Styles.baseMap, baseMapProperty, baseColorProperty);
        }
        
        // 绘制Normal属性
        internal static void DrawNormalProperties(MaterialEditor materialEditor, MaterialProperty bumpMap, MaterialProperty bumpMapScale = null)
        {
            if (bumpMapScale != null)
            {
                materialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap,
                    bumpMap.textureValue != null ? bumpMapScale : null);
                if (bumpMapScale.floatValue != 1 &&
                    UnityEditorInternal.InternalEditorUtility.IsMobilePlatform(
                        EditorUserBuildSettings.activeBuildTarget))
                    if (materialEditor.HelpBoxWithButton(Styles.bumpScaleNotSupported, Styles.fixNormalNow))
                        bumpMapScale.floatValue = 1;
            }
            else
            {
                materialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap);
            }
        }
        
        // 绘制自发光属性
        internal static void DrawEmissionProperties(MaterialEditor materialEditor, MaterialProperty emissionMapProperty, MaterialProperty emissionColorProperty, bool keyword)
        {
            var emissive = true;

            if (!keyword)
            {
                if ((emissionMapProperty == null) || (emissionColorProperty == null))
                    return;
                using (new EditorGUI.IndentLevelScope(2))
                {
                    materialEditor.TexturePropertyWithHDRColor(Styles.emissionMap, emissionMapProperty, emissionColorProperty, false);
                }
            }
            else
            {
                emissive = materialEditor.EmissionEnabledProperty();
                using (new EditorGUI.DisabledScope(!emissive))
                {
                    if ((emissionMapProperty == null) || (emissionColorProperty == null))
                        return;
                    using (new EditorGUI.IndentLevelScope(2))
                    {
                        materialEditor.TexturePropertyWithHDRColor(Styles.emissionMap, emissionMapProperty, emissionColorProperty, false);
                    }
                }
            }

            // If texture was assigned and color was black set color to white
            if ((emissionMapProperty != null) && (emissionColorProperty != null))
            {
                var hadEmissionTexture = emissionMapProperty?.textureValue != null;
                var brightness = emissionColorProperty.colorValue.maxColorComponent;
                if (emissionMapProperty.textureValue != null && !hadEmissionTexture && brightness <= 0f)
                    emissionColorProperty.colorValue = Color.white;
            }

            if (emissive)
            {
                // Change the GI emission flag and fix it up with emissive as black if necessary.
                materialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
            }
        }
        internal static void DrawTileOffset(MaterialEditor materialEditor, MaterialProperty textureProperty)
        {
            if (textureProperty != null)
                materialEditor.TextureScaleOffsetProperty(textureProperty);
        }
        
        internal static void DrawShaderGraphProperties(MaterialEditor materialEditor, IEnumerable<MaterialProperty> properties)
        {
            if (properties == null)
                return;
            ShaderGraphPropertyDrawers.DrawShaderGraphGUI(materialEditor, properties);
        }

        internal static void DrawFloatToggleProperty(GUIContent styles, MaterialProperty prop, int indentLevel = 0, bool isDisabled = false)
        {
            if (prop == null)
                return;

            EditorGUI.BeginDisabledGroup(isDisabled);
            EditorGUI.indentLevel += indentLevel;
            EditorGUI.BeginChangeCheck();
            MaterialEditor.BeginProperty(prop);
            bool newValue = EditorGUILayout.Toggle(styles, prop.floatValue == 1);
            if (EditorGUI.EndChangeCheck())
                prop.floatValue = newValue ? 1.0f : 0.0f;
            MaterialEditor.EndProperty();
            EditorGUI.indentLevel -= indentLevel;
            EditorGUI.EndDisabledGroup();
        }
        
        public static void TwoFloatSingleLine(GUIContent title, MaterialProperty prop1, GUIContent prop1Label,
            MaterialProperty prop2, GUIContent prop2Label, MaterialEditor materialEditor, float labelWidth = 30f)
        {
            const int kInterFieldPadding = 2;

            MaterialEditor.BeginProperty(prop1);
            MaterialEditor.BeginProperty(prop2);

            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.PrefixLabel(rect, title);

            var indent = EditorGUI.indentLevel;
            var preLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = labelWidth;

            Rect propRect1 = new Rect(rect.x + preLabelWidth, rect.y,
                (rect.width - preLabelWidth) * 0.5f - 1, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop1.hasMixedValue;
            var prop1val = EditorGUI.FloatField(propRect1, prop1Label, prop1.floatValue);
            if (EditorGUI.EndChangeCheck())
                prop1.floatValue = prop1val;

            Rect propRect2 = new Rect(propRect1.x + propRect1.width + kInterFieldPadding, rect.y,
                propRect1.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop2.hasMixedValue;
            var prop2val = EditorGUI.FloatField(propRect2, prop2Label, prop2.floatValue);
            if (EditorGUI.EndChangeCheck())
                prop2.floatValue = prop2val;

            EditorGUI.indentLevel = indent;
            EditorGUIUtility.labelWidth = preLabelWidth;

            EditorGUI.showMixedValue = false;

            MaterialEditor.EndProperty();
            MaterialEditor.EndProperty();
        }
                
        public static Rect TextureColorProperties(MaterialEditor materialEditor, GUIContent label, MaterialProperty textureProp, MaterialProperty colorProp, bool hdr = false)
        {
            MaterialEditor.BeginProperty(textureProp);
            if (colorProp != null)
                MaterialEditor.BeginProperty(colorProp);

            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.showMixedValue = textureProp.hasMixedValue;
            materialEditor.TexturePropertyMiniThumbnail(rect, textureProp, label.text, label.tooltip);
            EditorGUI.showMixedValue = false;

            if (colorProp != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = colorProp.hasMixedValue;
                int indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                Rect rectAfterLabel = new Rect(rect.x + EditorGUIUtility.labelWidth, rect.y,
                    EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight);
                var col = EditorGUI.ColorField(rectAfterLabel, GUIContent.none, colorProp.colorValue, true,
                    false, hdr);
                EditorGUI.indentLevel = indentLevel;
                if (EditorGUI.EndChangeCheck())
                {
                    materialEditor.RegisterPropertyChangeUndo(colorProp.displayName);
                    colorProp.colorValue = col;
                }
                EditorGUI.showMixedValue = false;
            }

            if (colorProp != null)
                MaterialEditor.EndProperty();
            MaterialEditor.EndProperty();

            return rect;
        }
    }
}