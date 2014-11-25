// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.ReferenceCounting;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A renderable texture view.
    /// </summary>
    public partial class RenderTarget : GraphicsResourceBase
    {
        public readonly TextureDescription Description;

        /// <summary>
        /// The underlying texture.
        /// </summary>
        public readonly Texture Texture;

        /// <summary>
        /// Gets the width in texel.
        /// </summary>
        /// <value>The width.</value>
        public int Width;

        /// <summary>
        /// Gets the height in texel.
        /// </summary>
        /// <value>The height.</value>
        public int Height;

        /// <summary>
        /// The format of this texture view.
        /// </summary>
        public readonly PixelFormat ViewFormat;

        /// <summary>
        /// The format of this texture view.
        /// </summary>
        public readonly ViewType ViewType;

        /// <summary>
        /// The miplevel index of this texture view.
        /// </summary>
        public readonly int MipLevel;

        /// <summary>
        /// The array index of this texture view.
        /// </summary>
        public readonly int ArraySlice;

        protected override void Destroy()
        {
            base.Destroy();
            Texture.ReleaseInternal();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="RenderTarget"/> to <see cref="Texture"/>.
        /// </summary>
        /// <param name="renderTarget">The render target.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Texture(RenderTarget renderTarget)
        {
            return renderTarget == null ? null : renderTarget.Texture;
        }
    }
}
