using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.AdditionalData
{
    public class AdditionalCameraData : MonoBehaviour, ISerializationCallbackReceiver, IAdditionalData
    {
        public void OnBeforeSerialize()
        {
            throw new System.NotImplementedException();
        }

        public void OnAfterDeserialize()
        {
            throw new System.NotImplementedException();
        }
    }
}