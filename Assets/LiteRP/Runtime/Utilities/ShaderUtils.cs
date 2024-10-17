using System;
using UnityEngine;

namespace LiteRP
{
    public enum ShaderPathID
    {
        Unlit,
        Lit,
        ParticlesUnlit
    }
    public static class ShaderUtils
    {
        static readonly string[] s_ShaderPaths =
        {
            "LiteRenderPipeline/Unlit",
            "LiteRenderPipeline/Lit",
            "LiteRenderPipeline/ParticlesUnlit",
        };
        
        public static string GetShaderPath(ShaderPathID id)
        {
            int index = (int)id;
            int arrayLength = s_ShaderPaths.Length;
            if (arrayLength > 0 && index >= 0 && index < arrayLength)
                return s_ShaderPaths[index];

            Debug.LogError("Trying to access universal shader path out of bounds: (" + id + ": " + index + ")");
            return "";
        }
        
        public static ShaderPathID GetEnumFromPath(string path)
        {
            var index = Array.FindIndex(s_ShaderPaths, m => m == path);
            
            return (ShaderPathID)index;
        }
        
        public static bool IsLiteRPShader(Shader shader)
        {
            return Array.Exists(s_ShaderPaths, m => m == shader.name);
        }
#if UNITY_EDITOR
        private static float s_MostRecentValidDeltaTime = 0.0f;
#endif
        // 在进入暂停模式或使用FrameDebugger时delta time不会重置为0，除非Time.timeScale为0
        // * 在Domain重新加载后的第一帧可以为0 (如果Time.deltaTime也为0)
        // * 该值取决于上次调用的时间
        // * 在实践中，它不应该过时，因为它在LiteRP帧中至少被调用一次
        // * 当前仅被使用在Shader upload时计算“_LastTimeParameters”
        // * 如果试图在其他地方重用此用例，请验证您的用例（因为它可能不会传输）
        internal static float PersistentDeltaTime
        {
            get
            {
#if UNITY_EDITOR
                float deltaTime = Time.deltaTime;
                // 我所知道的deltaTime为0有效的唯一情况是Time.timeScale为0
                if (deltaTime > 0.0f || Time.timeScale == 0.0f)
                    s_MostRecentValidDeltaTime = deltaTime;
                return s_MostRecentValidDeltaTime;
#else
                return Time.deltaTime;
#endif
            }
        }
#if UNITY_EDITOR
        static readonly string[] s_ShaderGUIDs =
        {
            "a9316d8cae61a45d6bf26d5d6216b3c4",
            "33584a15c90854d78b2b83b6df0ee27d",
            "31f500cd09a64ceb9ec3b7ff041dc695",
        };

        /// <summary>
        /// Returns a GUID for a URP shader from Shader Path ID.
        /// </summary>
        /// <param name="id">ID of shader path.</param>
        /// <returns>GUID for the shader.</returns>
        public static string GetShaderGUID(ShaderPathID id)
        {
            int index = (int)id;
            int arrayLength = s_ShaderGUIDs.Length;
            if (arrayLength > 0 && index >= 0 && index < arrayLength)
                return s_ShaderGUIDs[index];

            Debug.LogError("Trying to access universal shader GUID out of bounds: (" + id + ": " + index + ")");
            return "";
        }
#endif
    }
}