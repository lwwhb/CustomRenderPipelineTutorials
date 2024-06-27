using UnityEditor;
using UnityEngine;

namespace LiteRP.Editor
{
    internal static class LitShaderGUIHelper 
    {
        public static class Styles
        {
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
        }
        public static void DrawMetallicProperties(MaterialEditor materialEditor, Material material, 
            MaterialProperty metallicProperty, MaterialProperty metallicGlossMapProperty, 
            MaterialProperty smoothnessProperty, MaterialProperty smoothnessMapChannelProperty)
        {
            bool hasGlossMap = metallicGlossMapProperty.textureValue != null;
            materialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicGlossMapProperty,
                hasGlossMap ? null : metallicProperty);
            DrawSmoothness(materialEditor, material, smoothnessProperty, smoothnessMapChannelProperty);
        }
        private static void DrawSmoothness(MaterialEditor materialEditor, Material material, MaterialProperty smoothness, MaterialProperty smoothnessMapChannel)
        {
            EditorGUI.indentLevel += 2;

            materialEditor.ShaderProperty(smoothness, Styles.smoothnessText);

            if (smoothnessMapChannel != null) // smoothness channel
            {
                var opaque = LitShaderHelper.IsOpaque(material);
                EditorGUI.indentLevel++;
                EditorGUI.showMixedValue = smoothnessMapChannel.hasMixedValue;
                if (opaque)
                {
                    MaterialEditor.BeginProperty(smoothnessMapChannel);
                    EditorGUI.BeginChangeCheck();
                    var smoothnessSource = (int)smoothnessMapChannel.floatValue;
                    smoothnessSource = EditorGUILayout.Popup(Styles.smoothnessMapChannelText, smoothnessSource, Styles.metallicSmoothnessChannelNames);
                    if (EditorGUI.EndChangeCheck())
                        smoothnessMapChannel.floatValue = smoothnessSource;
                    MaterialEditor.EndProperty();
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Popup(Styles.smoothnessMapChannelText, 0, Styles.metallicSmoothnessChannelNames);
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
    }
}