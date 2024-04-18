using UnityEditor;

namespace LiteRP.Editor
{
    [CustomEditor(typeof(LiteRPAsset)), CanEditMultipleObjects]
    public class LiteRPAssetEditor : UnityEditor.Editor
    {
        private SerializedLiteRPAssetProperties m_SerializedLiteRPAssetProperties;
        void OnEnable()
        {
            m_SerializedLiteRPAssetProperties = new SerializedLiteRPAssetProperties(serializedObject);
        }
        public override void OnInspectorGUI()
        {
            m_SerializedLiteRPAssetProperties.Update();
            LiteRPAssetGUIHelper.Inspector.Draw(m_SerializedLiteRPAssetProperties, this);
            m_SerializedLiteRPAssetProperties.Apply();
        }
    }
}