using UnityEditor;
using UnityEngine;

namespace LiteRP.Editor
{
    internal static class LitShaderGUIHelper 
    {
        public static class Styles
        {
            /// <summary>
            /// The text and tooltip for the workflow Mode GUI.
            /// </summary>
            public static GUIContent workflowModeText = EditorGUIUtility.TrTextContent("Workflow Mode",
                "Select a workflow that fits your textures. Choose between Metallic or Specular.");
            
            /// <summary>
            /// The text and tooltip for the specular Map GUI.
            /// </summary>
            public static GUIContent specularMapText = EditorGUIUtility.TrTextContent("Specular Map", 
                "Designates a Specular Map and specular color determining the apperance of reflections on this Material's surface.");
            
            /// <summary>
            /// The text and tooltip for the metallic Map GUI.
            /// </summary>
            public static GUIContent metallicMapText =
                EditorGUIUtility.TrTextContent("Metallic Map", "Sets and configures the map for the Metallic workflow.");

            /// <summary>
            /// The text and tooltip for the smoothness GUI.
            /// </summary>
            public static GUIContent smoothnessText = EditorGUIUtility.TrTextContent("Smoothness",
                "Controls the spread of highlights and reflections on the surface.");

            /// <summary>
            /// The text and tooltip for the smoothness source GUI.
            /// </summary>
            public static GUIContent smoothnessMapChannelText =
                EditorGUIUtility.TrTextContent("Source",
                    "Specifies where to sample a smoothness map from. By default, uses the alpha channel for your map.");

            /// <summary>
            /// The text and tooltip for the specular Highlights GUI.
            /// </summary>
            public static GUIContent highlightsText = EditorGUIUtility.TrTextContent("Specular Highlights",
                "When enabled, the Material reflects the shine from direct lighting.");

            /// <summary>
            /// The text and tooltip for the environment Reflections GUI.
            /// </summary>
            public static GUIContent reflectionsText =
                EditorGUIUtility.TrTextContent("Environment Reflections",
                    "When enabled, the Material samples reflections from the nearest Reflection Probes or Lighting Probe.");
            
            /// <summary>
            /// The text and tooltip for the optimized BRDF GUI.
            /// </summary>
            public static GUIContent optimizedBRDFText =
                EditorGUIUtility.TrTextContent("Optimized BRDF",
                    "When enabled, use URP BRDF or use Disney BRDF.");

            /// <summary>
            /// The text and tooltip for the height map GUI.
            /// </summary>
            public static GUIContent heightMapText = EditorGUIUtility.TrTextContent("Height Map",
                "Defines a Height Map that will drive a parallax effect in the shader making the surface seem displaced.");

            /// <summary>
            /// The text and tooltip for the occlusion map GUI.
            /// </summary>
            public static GUIContent occlusionText = EditorGUIUtility.TrTextContent("Occlusion Map",
                "Sets an occlusion map to simulate shadowing from ambient lighting.");

            /// <summary>
            /// The names for smoothness alpha options available for metallic workflow.
            /// </summary>
            public static readonly string[] metallicSmoothnessChannelNames = { "Metallic Alpha", "Albedo Alpha" };
            /// <summary>
            /// The names for smoothness alpha options available for specular workflow.
            /// </summary>
            public static readonly string[] specularSmoothnessChannelNames = { "Specular Alpha", "Albedo Alpha" };
            
            /// <summary>
            /// The text and tooltip for the enabling/disabling clear coat GUI.
            /// </summary>
            public static GUIContent clearCoatText = EditorGUIUtility.TrTextContent("Clear Coat",
                "A multi-layer material feature which simulates a thin layer of coating on top of the surface material." +
                "\nPerformance cost is considerable as the specular component is evaluated twice, once per layer.");

            /// <summary>
            /// The text and tooltip for the clear coat Mask GUI.
            /// </summary>
            public static GUIContent clearCoatMaskText = EditorGUIUtility.TrTextContent("Mask",
                "Specifies the amount of the coat blending." +
                "\nActs as a multiplier of the clear coat map mask value or as a direct mask value if no map is specified." +
                "\nThe map specifies clear coat mask in the red channel and clear coat smoothness in the green channel.");

            /// <summary>
            /// The text and tooltip for the clear coat smoothness GUI.
            /// </summary>
            public static GUIContent clearCoatSmoothnessText = EditorGUIUtility.TrTextContent("Smoothness",
                "Specifies the smoothness of the coating." +
                "\nActs as a multiplier of the clear coat map smoothness value or as a direct smoothness value if no map is specified.");
        }
        public static void DrawMetallicSpecularProperties(MaterialEditor materialEditor, 
            Material material, MaterialProperty workflowProperty, 
            MaterialProperty metallicProperty, MaterialProperty metallicGlossMapProperty, 
            MaterialProperty specularProperty, MaterialProperty specularGlossMapProperty,
            MaterialProperty smoothnessProperty, MaterialProperty smoothnessMapChannelProperty)
        {
            string[] smoothnessChannelNames;
            bool hasGlossMap = false;
            if (workflowProperty == null ||
                (LitShaderHelper.WorkflowMode)workflowProperty.floatValue == LitShaderHelper.WorkflowMode.Metallic)
            {
                hasGlossMap = metallicGlossMapProperty.textureValue != null;
                smoothnessChannelNames = Styles.metallicSmoothnessChannelNames;
                materialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicGlossMapProperty,
                    hasGlossMap ? null : metallicProperty);
            }
            else
            {
                hasGlossMap = specularGlossMapProperty.textureValue != null;
                smoothnessChannelNames = Styles.specularSmoothnessChannelNames;
                LiteRPShaderGUIHelper.TextureColorProperties(materialEditor, Styles.specularMapText, specularGlossMapProperty,
                    hasGlossMap ? null : specularProperty);
            }
            DrawSmoothness(materialEditor, material, smoothnessProperty, smoothnessMapChannelProperty, smoothnessChannelNames);
        }
        
        private static void DrawSmoothness(MaterialEditor materialEditor, Material material, MaterialProperty smoothness, MaterialProperty smoothnessMapChannel, string[] smoothnessChannelNames)
        {
            EditorGUI.indentLevel += 2;

            materialEditor.ShaderProperty(smoothness, Styles.smoothnessText);

            if (smoothnessMapChannel != null) // smoothness channel
            {
                var opaque = LiteRPShaderHelper.IsOpaque(material);
                EditorGUI.indentLevel++;
                EditorGUI.showMixedValue = smoothnessMapChannel.hasMixedValue;
                if (opaque)
                {
                    MaterialEditor.BeginProperty(smoothnessMapChannel);
                    EditorGUI.BeginChangeCheck();
                    var smoothnessSource = (int)smoothnessMapChannel.floatValue;
                    smoothnessSource = EditorGUILayout.Popup(Styles.smoothnessMapChannelText, smoothnessSource, smoothnessChannelNames);
                    if (EditorGUI.EndChangeCheck())
                        smoothnessMapChannel.floatValue = smoothnessSource;
                    MaterialEditor.EndProperty();
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Popup(Styles.smoothnessMapChannelText, 0, smoothnessChannelNames);
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUI.showMixedValue = false;
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel -= 2;
        }
        
        public static void DrawHeightProperties(MaterialEditor materialEditor, MaterialProperty parallaxMapPropProperty,  MaterialProperty parallaxScalePropProperty)
        {
            materialEditor.TexturePropertySingleLine(Styles.heightMapText, parallaxMapPropProperty,
                parallaxMapPropProperty.textureValue != null ? parallaxScalePropProperty : null);
        }
        
        public static void DrawOcclusionProperties(MaterialEditor materialEditor, MaterialProperty occlusionMapProperty, MaterialProperty occlusionStrengthProperty)
        {
            materialEditor.TexturePropertySingleLine(Styles.occlusionText, occlusionMapProperty,
                occlusionMapProperty.textureValue != null ? occlusionStrengthProperty : null);
        }
        
        /// <summary>
        /// Draws the clear coat GUI.
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="materialEditor"></param>
        /// <param name="material"></param>
        public static void DrawClearCoatProperties(MaterialEditor materialEditor, MaterialProperty clearCoatProperty, 
            MaterialProperty clearCoatMapProperty, MaterialProperty clearCoatMaskProperty, MaterialProperty clearCoatSmoothnessProperty)
        {
            materialEditor.ShaderProperty(clearCoatProperty, Styles.clearCoatText);
            var coatEnabled = clearCoatProperty.floatValue> 0.0;
            if(coatEnabled)
            {
                EditorGUI.indentLevel += 2;
                materialEditor.TexturePropertySingleLine(Styles.clearCoatMaskText, clearCoatMapProperty, clearCoatMaskProperty);

                // Texture and HDR color controls
                materialEditor.ShaderProperty(clearCoatSmoothnessProperty, Styles.clearCoatSmoothnessText);

                EditorGUI.indentLevel -= 2;
            }
        }
    }
}