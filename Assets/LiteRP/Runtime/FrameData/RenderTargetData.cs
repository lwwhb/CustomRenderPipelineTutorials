using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP.FrameData
{
    public class RenderTargetData : ResourceData
    {
        //获取RT纹理资源
        protected TextureHandle GetTextureHandle(ref TextureHandle handle)
        {
            if (!CheckAndWarnAboutAccessibility())
                return TextureHandle.nullHandle;

            return handle;
        }
        
        //设置RT纹理资源
        protected void SetTextureHandle(ref TextureHandle handle, TextureHandle newHandle)
        {
            if (!CheckAndWarnAboutAccessibility())
                return;

            handle = newHandle;
        }
        
        ///---RT纹理资源
        // 用于直接渲染到屏幕的后备颜色缓冲区。根据帧设置，所有RenderGraphPass都可以写入它。
        public TextureHandle backBufferColor
        {
            get => GetTextureHandle(ref _backBufferColor);
            internal set => SetTextureHandle(ref _backBufferColor, value);
        }
        private TextureHandle _backBufferColor;
        
        // 用于直接渲染到屏幕的后备深度缓冲区深度。根据帧设置，所有RenderGraphPass都可以写入它。
        public TextureHandle backBufferDepth
        {
            get => GetTextureHandle(ref _backBufferDepth);
            internal set => SetTextureHandle(ref _backBufferDepth, value);
        }
        private TextureHandle _backBufferDepth;
        
        // 用于主光源阴影渲染
        public TextureHandle mainLightShadow
        {
            get => GetTextureHandle(ref _mainLightShadow);
            internal set => SetTextureHandle(ref _mainLightShadow, value);
        }
        private TextureHandle _mainLightShadow;
        //---

        public override void Reset()
        {
            _backBufferColor = TextureHandle.nullHandle;
            _backBufferDepth = TextureHandle.nullHandle;
            
            _mainLightShadow = TextureHandle.nullHandle;
        }
    }
}