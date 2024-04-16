using System;
using UnityEditor;
using UnityEngine;

namespace LiteRP.Editor
{
    internal class UnlitShaderGUI : LiteRPShaderGUI
    {
        // 材质改变时的回调
        public override void ValidateMaterial(Material material)
        {
            LiteRPShaderHelper.SetMaterialKeywords(material);
        }
        // 替换Shader时回调
        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // 清空材质关键字来刷新
            material.shaderKeywords = null;
            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null)
            {
                LiteRPShaderHelper.SetupMaterialBlendMode(material);
            }
            else
            {
                SurfaceType surfaceType = (SurfaceType)material.GetFloat("_Surface");
                BlendMode blendMode = (BlendMode)material.GetFloat("_Blend");
            
                if (surfaceType == SurfaceType.Opaque)
                {
                    material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                }
                else
                {
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                }
            }
        }
    }
}