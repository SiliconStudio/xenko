// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL 
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
    /// A renderable Texture2D.
    /// </summary>
    public partial class RenderTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTarget2D"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="name">The name of this texture.</param>
        /// <param name="description">The description.</param>
        /// <param name="initialize">if set to <c>true</c> [initialize].</param>
        internal RenderTarget(GraphicsDevice device, Texture texture, ViewType viewType, int arraySlize, int mipSlice, PixelFormat viewFormat = PixelFormat.None)
            : base(device)
        {
            texture.AddReferenceInternal();
            Texture = texture;
            Description = texture.Description;
            resourceId = texture.ResourceId;

            Width = Math.Max(1, Description.Width >> mipSlice);
            Height = Math.Max(1, Description.Height >> mipSlice);

            ViewType = viewType;
            ArraySlice = arraySlize;
            MipLevel = mipSlice;
            ViewFormat = viewFormat == PixelFormat.None ? Description.Format : viewFormat;
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
    }
}
 
#endif
