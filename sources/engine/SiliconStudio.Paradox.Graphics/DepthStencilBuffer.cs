// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.ReferenceCounting;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Depth stencil buffer
    /// </summary>
    public partial class DepthStencilBuffer : GraphicsResourceBase
    {
        internal TextureDescription DescriptionInternal;

        public TextureDescription Description
        {
            get { return DescriptionInternal; }
        }

        public readonly Texture2D Texture;

        protected override void Destroy()
        {
            base.Destroy();
            Texture.ReleaseInternal();
        }
    }
}
