using System;
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
        
        //激活的Color target ID
        internal ActiveID activeColorID { get; set; }

        //激活的颜色纹理资源
        public TextureHandle activeColorTexture
        {
            get
            {
                if (!CheckAndWarnAboutAccessibility())
                    return TextureHandle.nullHandle;

                switch (activeColorID)
                {
                    case ActiveID.FrontBuffer:
                        return frontBufferColor;
                    case ActiveID.BackBuffer:
                        return backBufferColor;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        //激活的Depth target ID
        internal ActiveID activeDepthID { get; set; }
        
        //激活的深度纹理资源
        public TextureHandle activeDepthTexture
        {
            get
            {
                if (!CheckAndWarnAboutAccessibility())
                    return TextureHandle.nullHandle;

                switch (activeDepthID)
                {
                    case ActiveID.FrontBuffer:
                        return frontBufferDepth;
                    case ActiveID.BackBuffer:
                        return backBufferDepth;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
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
        
        // 用于相机的颜色与深度的前端缓冲区。
        public TextureHandle frontBufferColor
        {
            get => GetTextureHandle(ref _frontBufferColor);
            internal set => SetTextureHandle(ref _frontBufferColor, value);
        }
        private TextureHandle _frontBufferColor;
        
        public TextureHandle frontBufferDepth
        {
            get => GetTextureHandle(ref _frontBufferDepth);
            internal set => SetTextureHandle(ref _frontBufferDepth, value);
        }
        private TextureHandle _frontBufferDepth;
        //---
        
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
            
            _frontBufferColor = TextureHandle.nullHandle;
            _frontBufferDepth = TextureHandle.nullHandle;
            
            _mainLightShadow = TextureHandle.nullHandle;
        }
    }
}