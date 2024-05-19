using LiteRP.AdditionalData;
using UnityEditor;

namespace LiteRP.Editor
{
    [CustomEditor(typeof(AdditionalLightData))]
    [CanEditMultipleObjects]
    public class AdditionalLightDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
        }
    }
}