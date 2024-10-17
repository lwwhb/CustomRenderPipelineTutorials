
using LiteRP.Editor.ShaderGraph;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace LiteRP.Editor
{
    public static class ShaderUtils
    {
        internal enum ShaderID
        {
            Unknown = -1,

            Unlit = ShaderPathID.Unlit,
            Lit = ShaderPathID.Lit,
            ParticlesUnlit = ShaderPathID.ParticlesUnlit,


            // ShaderGraph IDs start at 1000, correspond to subtargets
            SG_Unlit = 1000, // LiteRPUnlitSubTarget
            SG_Lit, // LiteRPLitSubTarget
        }

        internal static bool IsShaderGraph(this ShaderID id)
        {
            return ((int)id >= 1000);
        }

        internal static ShaderID GetShaderID(Shader shader)
        {
            if (shader.IsShaderGraphAsset())
            {
                LiteRPMetadata meta;
                if (!shader.TryGetMetadataOfType<LiteRPMetadata>(out meta))
                    return ShaderID.Unknown;
                return meta.shaderID;
            }
            else
            {
                ShaderPathID pathID = LiteRP.ShaderUtils.GetEnumFromPath(shader.name);
                return (ShaderID)pathID;
            }
        }

        internal enum MaterialUpdateType
        {
            CreatedNewMaterial,
            ChangedAssignedShader,
            ModifiedShader,
            ModifiedMaterial
        }

        //Helper used by VFX, allow retrieval of ShaderID on another object than material.shader
        //In case of ShaderGraph integration, the material.shader is actually pointing to VisualEffectAsset
        internal static void UpdateMaterial(Material material, MaterialUpdateType updateType,
            UnityEngine.Object assetWithLiteRPMetaData)
        {
            var currentShaderId = ShaderUtils.ShaderID.Unknown;
            if (assetWithLiteRPMetaData != null)
            {
                var path = AssetDatabase.GetAssetPath(assetWithLiteRPMetaData);
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is LiteRPMetadata metadataAsset)
                    {
                        currentShaderId = metadataAsset.shaderID;
                        break;
                    }
                }
            }

            UpdateMaterial(material, updateType, currentShaderId);
        }
        
        internal static void UpdateMaterial(Material material, MaterialUpdateType updateType,
            ShaderID shaderID = ShaderID.Unknown)
        {
            // if unknown, look it up from the material's shader
            // NOTE: this will only work for asset-based shaders..
            if (shaderID == ShaderID.Unknown)
                shaderID = GetShaderID(material.shader);

            switch (shaderID)
            {
                case ShaderID.Unlit:
                    LiteRPShaderHelper.SetMaterialKeywords(material);
                    break;
                case ShaderID.Lit:
                    LiteRPShaderHelper.SetMaterialKeywords(material, LitShaderHelper.SetMaterialKeywords);
                    break;
                case ShaderID.ParticlesUnlit:
                    LiteRPShaderHelper.SetMaterialKeywords(material, ParticlesShaderHelper.SetMaterialKeywords);
                    break;
                case ShaderID.SG_Unlit:
                    break;
                case ShaderID.SG_Lit:
                    break;
                default:
                    break;
            }
        }
    }
}