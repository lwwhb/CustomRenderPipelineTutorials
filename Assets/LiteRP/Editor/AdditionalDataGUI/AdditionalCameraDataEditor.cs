using LiteRP.AdditionalData;
using UnityEditor;

namespace LiteRP.Editor
{
    [CustomEditor(typeof(AdditionalCameraData))]
    [CanEditMultipleObjects]
    public class AdditionalCameraDataEditor  : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
        }
    }
}