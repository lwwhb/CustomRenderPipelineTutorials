using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Editor
{
    internal static class ParticlesShaderGUIHelper 
    {
        /// <summary>
        /// Container for the text and tooltips used to display the shader.
        /// </summary>
        public static class Styles
        {
            /// <summary>
            /// The text and tooltip color mode.
            /// </summary>
            public static GUIContent colorMode = EditorGUIUtility.TrTextContent("Color Mode",
                "Controls how the Particle color and the Material color blend together.");

            /// <summary>
            /// The text and tooltip flip-book blending.
            /// </summary>
            public static GUIContent flipbookMode = EditorGUIUtility.TrTextContent("Flip-Book Blending",
                "Blends the frames in a flip-book together in a smooth animation.");

            /// <summary>
            /// The text and tooltip soft particles.
            /// </summary>
            public static GUIContent softParticlesEnabled = EditorGUIUtility.TrTextContent("Soft Particles",
                "Makes particles fade out when they get close to intersecting with the surface of other geometry in the depth buffer.");

            /// <summary>
            /// The text and tooltip soft particles surface fade.
            /// </summary>
            public static GUIContent softParticlesFadeText = EditorGUIUtility.TrTextContent("Surface Fade");

            /// <summary>
            /// The text and tooltip soft particles near fade distance.
            /// </summary>
            public static GUIContent softParticlesNearFadeDistanceText =
                EditorGUIUtility.TrTextContent("Near",
                    "The distance from the other surface where the particle is completely transparent.");

            /// <summary>
            /// The text and tooltip soft particles far fade distance.
            /// </summary>
            public static GUIContent softParticlesFarFadeDistanceText =
                EditorGUIUtility.TrTextContent("Far",
                    "The distance from the other surface where the particle is completely opaque.");

            /// <summary>
            /// The text and tooltip camera fading.
            /// </summary>
            public static GUIContent cameraFadingEnabled = EditorGUIUtility.TrTextContent("Camera Fading",
                "Makes particles fade out when they get close to the camera.");

            /// <summary>
            /// The text and tooltip camera fading distance.
            /// </summary>
            public static GUIContent cameraFadingDistanceText = EditorGUIUtility.TrTextContent("Distance");

            /// <summary>
            /// The text and tooltip camera fading near distance.
            /// </summary>
            public static GUIContent cameraNearFadeDistanceText =
                EditorGUIUtility.TrTextContent("Near",
                    "The distance from the camera where the particle is completely transparent.");

            /// <summary>
            /// The text and tooltip camera fading far distance.
            /// </summary>
            public static GUIContent cameraFarFadeDistanceText =
                EditorGUIUtility.TrTextContent("Far", "The distance from the camera where the particle is completely opaque.");

            /// <summary>
            /// The text and tooltip distortion.
            /// </summary>
            public static GUIContent distortionEnabled = EditorGUIUtility.TrTextContent("Distortion",
                "Creates a distortion effect by making particles perform refraction with the objects drawn before them.");

            /// <summary>
            /// The text and tooltip distortion strength.
            /// </summary>
            public static GUIContent distortionStrength = EditorGUIUtility.TrTextContent("Strength",
                "Controls how much the Particle distorts the background. ");

            /// <summary>
            /// The text and tooltip distortion blend.
            /// </summary>
            public static GUIContent distortionBlend = EditorGUIUtility.TrTextContent("Blend",
                "Controls how visible the distortion effect is. At 0, there’s no visible distortion. At 1, only the distortion effect is visible, not the background.");

            /// <summary>
            /// The text and tooltip for vertex streams.
            /// </summary>
            public static GUIContent VertexStreams = EditorGUIUtility.TrTextContent("Vertex Streams",
                "List detailing the expected layout of data sent to the shader from the particle system.");

            /// <summary>
            /// The string for position vertex stream.
            /// </summary>
            public static string streamPositionText = "Position (POSITION.xyz)";

            /// <summary>
            /// The string for normal vertex stream.
            /// </summary>
            public static string streamNormalText = "Normal (NORMAL.xyz)";

            /// <summary>
            /// The string for color vertex stream.
            /// </summary>
            public static string streamColorText = "Color (COLOR.xyzw)";

            /// <summary>
            /// The string for color instanced vertex stream.
            /// </summary>
            public static string streamColorInstancedText = "Color (INSTANCED0.xyzw)";

            /// <summary>
            /// The string for UV vertex stream.
            /// </summary>
            public static string streamUVText = "UV (TEXCOORD0.xy)";

            /// <summary>
            /// The string for UV2 vertex stream.
            /// </summary>
            public static string streamUV2Text = "UV2 (TEXCOORD0.zw)";

            /// <summary>
            /// The string for AnimBlend Texcoord1 vertex stream.
            /// </summary>
            public static string streamAnimBlendText = "AnimBlend (TEXCOORD1.x)";

            /// <summary>
            /// The string for AnimBlend Instanced1 vertex stream.
            /// </summary>
            public static string streamAnimFrameText = "AnimFrame (INSTANCED1.x)";

            /// <summary>
            /// The string for tangent vertex stream.
            /// </summary>
            public static string streamTangentText = "Tangent (TANGENT.xyzw)";

            /// <summary>
            /// The text and tooltip for the vertex stream fix now GUI.
            /// </summary>
            public static GUIContent streamApplyToAllSystemsText = EditorGUIUtility.TrTextContent("Fix Now",
                "Apply the vertex stream layout to all Particle Systems using this material");

            /// <summary>
            /// The string for applying custom vertex streams from material.
            /// </summary>
            public static string undoApplyCustomVertexStreams = L10n.Tr("Apply custom vertex streams from material");

            /// <summary>
            /// The vertex stream icon.
            /// </summary>
            public static GUIStyle vertexStreamIcon = new GUIStyle();
        }

        private static ReorderableList vertexStreamList;
        private static bool CompareVertexStreams(IEnumerable<ParticleSystemVertexStream> a, IEnumerable<ParticleSystemVertexStream> b)
        {
            var differenceA = a.Except(b);
            var differenceB = b.Except(a);
            var difference = differenceA.Union(differenceB).Distinct();
            if (!difference.Any())
                return true;
            // If normals are the only difference, ignore them, because the default particle streams include normals, to make it easy for users to switch between lit and unlit
            if (difference.Count() == 1)
            {
                if (difference.First() == ParticleSystemVertexStream.Normal)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Draws the vertex streams area.
        /// </summary>
        /// <param name="material">The material to use.</param>
        /// <param name="renderers">List of particle system renderers.</param>
        /// <param name="useLighting">Marks whether the renderers uses lighting or not.</param>
        public static void DoVertexStreamsArea(Material material, List<ParticleSystemRenderer> renderers, bool useLighting = false)
        {
            EditorGUILayout.Space();
            // Display list of streams required to make this shader work
            bool useNormalMap = false;
            bool useFlipbookBlending = (material.GetFloat("_FlipbookBlending") > 0.0f);
            if (material.HasProperty("_BumpMap"))
                useNormalMap = material.GetTexture("_BumpMap");

            bool useGPUInstancing = ShaderUtil.HasProceduralInstancing(material.shader);
            if (useGPUInstancing && renderers.Count > 0)
            {
                if (!renderers[0].enableGPUInstancing || renderers[0].renderMode != ParticleSystemRenderMode.Mesh)
                    useGPUInstancing = false;
            }

            // Build the list of expected vertex streams
            List<ParticleSystemVertexStream> streams = new List<ParticleSystemVertexStream>();
            List<string> streamList = new List<string>();

            streams.Add(ParticleSystemVertexStream.Position);
            streamList.Add(Styles.streamPositionText);

            if (useLighting || useNormalMap)
            {
                streams.Add(ParticleSystemVertexStream.Normal);
                streamList.Add(Styles.streamNormalText);
                if (useNormalMap)
                {
                    streams.Add(ParticleSystemVertexStream.Tangent);
                    streamList.Add(Styles.streamTangentText);
                }
            }

            streams.Add(ParticleSystemVertexStream.Color);
            streamList.Add(useGPUInstancing ? Styles.streamColorInstancedText : Styles.streamColorText);
            streams.Add(ParticleSystemVertexStream.UV);
            streamList.Add(Styles.streamUVText);

            List<ParticleSystemVertexStream> instancedStreams = new List<ParticleSystemVertexStream>(streams);

            if (useGPUInstancing)
            {
                instancedStreams.Add(ParticleSystemVertexStream.AnimFrame);
                streamList.Add(Styles.streamAnimFrameText);
            }
            else if (useFlipbookBlending && !useGPUInstancing)
            {
                streams.Add(ParticleSystemVertexStream.UV2);
                streamList.Add(Styles.streamUV2Text);
                streams.Add(ParticleSystemVertexStream.AnimBlend);
                streamList.Add(Styles.streamAnimBlendText);
            }

            vertexStreamList = new ReorderableList(streamList, typeof(string), false, true, false, false);

            vertexStreamList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, Styles.VertexStreams);
            };

            vertexStreamList.DoLayoutList();

            // Display a warning if any renderers have incorrect vertex streams
            string Warnings = "";
            List<ParticleSystemVertexStream> rendererStreams = new List<ParticleSystemVertexStream>();
            foreach (ParticleSystemRenderer renderer in renderers)
            {
                renderer.GetActiveVertexStreams(rendererStreams);

                bool streamsValid;
                if (useGPUInstancing && renderer.renderMode == ParticleSystemRenderMode.Mesh && renderer.supportsMeshInstancing)
                    streamsValid = CompareVertexStreams(rendererStreams, instancedStreams);
                else
                    streamsValid = CompareVertexStreams(rendererStreams, streams);

                if (!streamsValid)
                    Warnings += "-" + renderer.name + "\n";
            }

            if (!string.IsNullOrEmpty(Warnings))
            {
                EditorGUILayout.HelpBox(
                    "The following Particle System Renderers are using this material with incorrect Vertex Streams:\n" +
                    Warnings, MessageType.Error, true);
                // Set the streams on all systems using this material
                if (GUILayout.Button(Styles.streamApplyToAllSystemsText, EditorStyles.miniButton, GUILayout.ExpandWidth(true)))
                {
                    Undo.RecordObjects(renderers.Where(r => r != null).ToArray(), Styles.undoApplyCustomVertexStreams);

                    foreach (ParticleSystemRenderer renderer in renderers)
                    {
                        if (useGPUInstancing && renderer.renderMode == ParticleSystemRenderMode.Mesh && renderer.supportsMeshInstancing)
                            renderer.SetActiveVertexStreams(instancedStreams);
                        else
                            renderer.SetActiveVertexStreams(streams);
                    }
                }
            }
        }
        
        /// <summary>
        /// Draws the fading options GUI.
        /// </summary>
        /// <param name="material">The material to use.</param>
        /// <param name="materialEditor">The material editor to use.</param>
        /// <param name="properties">The particle properties to use.</param>
        public static void FadingOptions(Material material, MaterialEditor materialEditor,
            MaterialProperty softParticlesEnabled, MaterialProperty softParticlesNearFadeDistance, MaterialProperty softParticlesFarFadeDistance,
            MaterialProperty cameraFadingEnabled, MaterialProperty cameraNearFadeDistance, MaterialProperty cameraFarFadeDistance,
            MaterialProperty distortionEnabled, MaterialProperty distortionStrength, MaterialProperty distortionBlend)
        {
            // Z write doesn't work with fading
            bool hasZWrite = (material.GetFloat(LiteRPShaderProperty.ZWrite) > 0.0f);
            if (!hasZWrite)
            {
                // Soft Particles
                {
                    materialEditor.ShaderProperty(softParticlesEnabled, Styles.softParticlesEnabled);
                    if (softParticlesEnabled.floatValue >= 0.5f)
                    {
                        //lwwhb 待获取场景深度后实现
                        /*if (LiteRPUtils.asset != null && !LiteRPUtils.asset.supportsCameraDepthTexture)
                        {
                            GUIStyle warnStyle = new GUIStyle(GUI.skin.label);
                            warnStyle.fontStyle = FontStyle.BoldAndItalic;
                            warnStyle.wordWrap = true;
                            EditorGUILayout.HelpBox("Soft Particles require depth texture. Please enable \"Depth Texture\" in the Universal Render Pipeline settings.", MessageType.Warning);
                        }*/

                        EditorGUI.indentLevel++;
                        LiteRPShaderGUIHelper.TwoFloatSingleLine(Styles.softParticlesFadeText,
                            softParticlesNearFadeDistance,
                            Styles.softParticlesNearFadeDistanceText,
                            softParticlesFarFadeDistance,
                            Styles.softParticlesFarFadeDistanceText,
                            materialEditor);
                        EditorGUI.indentLevel--;
                    }
                }

                // Camera Fading
                {
                    materialEditor.ShaderProperty(cameraFadingEnabled, Styles.cameraFadingEnabled);
                    if (cameraFadingEnabled.floatValue >= 0.5f)
                    {
                        EditorGUI.indentLevel++;
                        LiteRPShaderGUIHelper.TwoFloatSingleLine(Styles.cameraFadingDistanceText,
                            cameraNearFadeDistance,
                            Styles.cameraNearFadeDistanceText,
                            cameraFarFadeDistance,
                            Styles.cameraFarFadeDistanceText,
                            materialEditor);
                        EditorGUI.indentLevel--;
                    }
                }

                // Distortion
                if (distortionEnabled != null)
                {
                    materialEditor.ShaderProperty(distortionEnabled, Styles.distortionEnabled);
                    if (distortionEnabled.floatValue >= 0.5f)
                    {
                        EditorGUI.indentLevel++;
                        materialEditor.ShaderProperty(distortionStrength, Styles.distortionStrength);
                        materialEditor.ShaderProperty(distortionBlend, Styles.distortionBlend);
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.showMixedValue = false;
            }
        }
    }
}
