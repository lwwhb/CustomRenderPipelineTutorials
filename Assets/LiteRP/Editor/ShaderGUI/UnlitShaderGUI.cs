using UnityEditor;
using UnityEngine;

namespace LiteRP.Editor
{
    public class UnlitShaderGUI : LiteShaderGUI
    {
        protected MaterialProperty m_SurfaceTypeProperty { get; set; }
        protected MaterialProperty m_BlendModeProperty { get; set; }
        protected override void OnMaterialGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            var material = materialEditor?.target as Material;
            if (material == null)
                return;
            m_SurfaceTypeProperty = FindProperty("_Surface", props);
            m_BlendModeProperty = FindProperty("_Blend", props);
            
        }
        //材质改变时的回调
        public override void ValidateMaterial(Material material)
        {
            SetupMaterialBlendMode(material);
        }
    }
}