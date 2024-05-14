using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Editor
{
    [CustomEditor(typeof(Camera))]
    [SupportedOnRenderPipeline(typeof(LiteRPAsset))]
    [CanEditMultipleObjects]
    public class LiteRPCameraEditor : UnityEditor.Editor
    {
        SerializedLiteRPCameraProperties serializedCameraProperties { get; set; }
        
        CameraEditor.Settings m_Settings;
        protected CameraEditor.Settings settings => m_Settings ??= new CameraEditor.Settings(serializedObject);
        
        public void OnEnable()
        {
            serializedCameraProperties = new SerializedLiteRPCameraProperties(serializedObject, settings);
            serializedCameraProperties.Refresh();
            Undo.undoRedoPerformed += ReconstructReferenceToAdditionalDataSO;
        }
        public void OnDisable()
        {
            Undo.undoRedoPerformed -= ReconstructReferenceToAdditionalDataSO;
        }

        void ReconstructReferenceToAdditionalDataSO()
        {
            OnDisable();
            OnEnable();
        }
        
        public override void OnInspectorGUI()
        {
            serializedCameraProperties.Update();

            LiteRPCameraGUIHelper.Inspector.Draw(serializedCameraProperties, this);

            serializedCameraProperties.Apply();
        }
    }
}