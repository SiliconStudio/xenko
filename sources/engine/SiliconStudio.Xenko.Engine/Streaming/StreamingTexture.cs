// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Texture streaming object.
    /// </summary>
    public class StreamingTexture : StreamableResource
    {
        protected Texture _texture;
        protected int _residentMips;

        internal StreamingTexture(StreamingManager manager, ContentStorage storage, [NotNull] Texture texture)
            : base(manager, storage)
        {
            _texture = texture;
            _residentMips = 0;
        }

        /// <summary>
        /// Gets the texture object.
        /// </summary>
        public Texture Texture => _texture;

        /// <inheritdoc />
        public override int CurrentResidency => _residentMips;

        /// <inheritdoc />
        public override int AllocatedResidency => _texture.MipLevels;
    }
}
