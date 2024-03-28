using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.Rendering;

namespace LiteRP.Editor
{
    public class LiteRPShaderGUI : ShaderGUI
    {
        public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (!(RenderPipelineManager.currentPipeline is LiteRenderPipeline))
            {
                CoreEditorUtils.DrawFixMeBox("Editing LiteRP materials is only supported when an LiteRP asset is assigned in the Graphics Settings", MessageType.Warning, "Open",
                    () => SettingsService.OpenProjectSettings("Project/Graphics"));
            }
            else
            {
                OnMaterialGUI(materialEditor, properties);
            }
        }

        private void OnMaterialGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            
        }
    }
}