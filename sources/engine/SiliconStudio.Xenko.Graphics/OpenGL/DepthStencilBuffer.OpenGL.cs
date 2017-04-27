// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL 
// Copyright (c) 2011 Silicon Studio
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.ReferenceCounting;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Depth stencil buffer
    /// </summary>
    public partial class DepthStencilBuffer
    {
        private bool needReadOnlySynchronization;
        internal bool DepthMask = true;

        internal bool IsDepthBuffer { get; set; }
        internal bool IsStencilBuffer { get; set; }

        internal DepthStencilBuffer(GraphicsDevice device, Texture2D depthTexture, bool isReadOnly) : base(device)
        {
            DescriptionInternal = depthTexture.Description;
            depthTexture.AddReferenceInternal();
            Texture = depthTexture;

            resourceId = Texture.ResourceId;

            if (Description.Format == PixelFormat.D24_UNorm_S8_UInt ||
                Description.Format == PixelFormat.D32_Float_S8X24_UInt)
            {
                IsDepthBuffer = true;
                IsStencilBuffer = true;
            }
            else if (Description.Format == PixelFormat.D32_Float || 
                     Description.Format == PixelFormat.D16_UNorm)
            {
                IsDepthBuffer = true;
                IsStencilBuffer = false;
            }
            else 
                throw new NotSupportedException("The provided depth stencil format is currently not supported"); // implement composition for other formats


#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            if (isReadOnly)
            {
                if (device.versionMajor < 4)
                {
                    needReadOnlySynchronization = true;
                    throw new NotImplementedException();
                }
            }
#endif
        }

        /// <inheritdoc/>
        protected internal override bool OnRecreate()
        {
            // Dependency: wait for underlying texture to be recreated first
            if (Texture.LifetimeState != GraphicsResourceLifetimeState.Active)
                return false;

            base.OnRecreate();
            resourceId = Texture.ResourceId;

            return true;
        }

        protected override void DestroyImpl()
        {
            Texture.ReleaseInternal();

            resourceId = 0;

            base.DestroyImpl();
        }

        public static bool IsDepthStencilReadOnlySupported(GraphicsDevice device)
        {
            // Since OpenGL 4.0? (need to double-check)
            return (device.versionMajor >= 4);
        }
        
        public void SynchronizeReadOnly(Graphics.GraphicsDevice context)
        {
            if (needReadOnlySynchronization)
                throw new NotImplementedException();
        }
    }
}
 
#endif
