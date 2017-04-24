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

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingTexture"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="texture">The texture to stream.</param>
        public StreamingTexture(StreamingManager manager, [NotNull] Texture texture)
            : base(manager)
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
