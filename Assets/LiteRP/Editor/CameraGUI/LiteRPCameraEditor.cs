using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Editor
{
    [CustomEditor(typeof(Camera))]
    [SupportedOnRenderPipeline(typeof(LiteRPAsset))]
    [CanEditMultipleObjects]
    public class LiteRPCameraEditor : CameraEditor
    {
        
    }
}