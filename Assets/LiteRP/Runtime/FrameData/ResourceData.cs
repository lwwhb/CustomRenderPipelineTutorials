using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.FrameData
{
    public abstract class ResourceData : ContextItem
    {
        internal bool isAccessible { get; set; }

        internal void InitFrame()
        {
            isAccessible = true;
        }

        internal void EndFrame()
        {
            isAccessible = false;
        }

        //检查资源是否可以访问
        protected bool CheckAndWarnAboutAccessibility()
        {
            if (!isAccessible)
                Debug.LogError("Trying to access LiteRP Resources outside of the current frame setup.");

            return isAccessible;
        }
    }
}