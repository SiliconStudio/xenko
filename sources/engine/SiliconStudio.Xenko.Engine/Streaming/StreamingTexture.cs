// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Texture streaming object.
    /// </summary>
    public class StreamingTexture : StreamableResource
    {
        protected Texture _texture;
        protected ImageDescription _desc;
        protected int _residentMips;

        internal StreamingTexture(StreamingManager manager, [NotNull] Texture texture)
            : base(manager)
        {
            _texture = texture;
            _residentMips = 0;
        }

        /// <summary>
        /// Gets the texture object.
        /// </summary>
        public Texture Texture => _texture;

        /// <summary>
        /// Gets the texture image description (available in the storage container).
        /// </summary>
        public ImageDescription Description => _desc;

        /// <summary>
        /// Gets the total amount of mip levels.
        /// </summary>
        public int TotalMipLevels => _desc.MipLevels;

        /// <summary>
        /// Gets the width of maximum texture mip.
        /// </summary>
        public int TotalWidth => _desc.Width;

        /// <summary>
        /// Gets the height of maximum texture mip.
        /// </summary>
        public int TotalHeight => _desc.Height;

        /// <summary>
        /// Gets a value indicating whether this texture is a cube map.
        /// </summary>
        /// <value><c>true</c> if this texture is a cube map; otherwise, <c>false</c>.</value>
        public bool IsCubeMap => _desc.Dimension == TextureDimension.TextureCube;

        /// <summary>	
        /// Gets the texture texels format
        /// </summary>	
        public PixelFormat Format => _desc.Format;

        /// <inheritdoc />
        public override object Resource => _texture;

        /// <inheritdoc />
        public override int CurrentResidency => _residentMips;

        /// <inheritdoc />
        public override int AllocatedResidency => _texture.MipLevels;

        /// <inheritdoc />
        internal override bool CanBeUpdated => true;// TODO: check if there is no streaming tasks goin on for that texture

        internal void Init([NotNull] ContentStorage storage, ref ImageDescription imageDescription)
        {
            Init(storage);
            _desc = imageDescription;

            if (_texture.GraphicsDevice != null)
                _texture.OnDestroyed();
        }

        internal override Task UpdateAllocation(int residency)
        {
            Debug.Assert(MathUtil.IsInRange(residency, 0, TotalMipLevels));
            Task result = null;
            
            var allocatedResidency = AllocatedResidency;
            Debug.Assert(allocatedResidency >= 0);

            // Check if residency won't change
            if (residency == allocatedResidency)
            {
            }
            // Check if need to deallocate
            else if (residency == 0)
            {
                // Release texture memory
                _texture.OnDestroyed();
            }
            else
            {
                // Check if texture hasn't been allocated yet
                if (allocatedResidency == 0)
                {
                    // Create texture description
                    int mip = TotalMipLevels - residency;
                    int width = TotalWidth >> mip;
                    int height = TotalHeight >> mip;
                    TextureDescription desc;
                    if (IsCubeMap)
                    {
                        Debug.Assert(width == height);
                        desc = TextureDescription.NewCube(width, residency, Format, TextureFlags.ShaderResource);
                    }
                    else
                    {
                        desc = TextureDescription.New2D(width, height, residency, Format, TextureFlags.ShaderResource);
                    }

                    // Initialize texture
                    _texture.InitializeFrom(desc);
                }
                else
                {
                    // TODO: create async task to resize texture and copy contents to the new one (GPU async task)
                    throw new NotImplementedException("StreamingTexture.UpdateAllocation: resize texture allocation");
                }
            }

            return result;
        }
        
        internal override Task CreateStreamingTask(int residency)
        {
            // temporary code!
            
            // TODO: cache tasks? we need to get streaming tasks that reference any resources to se we detect if any task is running and dont update resource then

            return new Task(() =>
            {
                unsafe
                {
                    var dataBoxArray = new DataBox[_desc.MipLevels];
                    {
                        for (int mipIndex = 0; mipIndex < Description.MipLevels; mipIndex++)
                        {
                            //var pixelBuffer = this.GetPixelBufferUnsafe(arrayIndex, 0, mipIndex);

                            int w = TotalWidth >> mipIndex;
                            int h = TotalWidth >> mipIndex;

                            int rowPitch, slicePitch;
                            int widthPacked;
                            int heightPacked;
                            Image.ComputePitch(Format, w, h, out rowPitch, out slicePitch, out widthPacked, out heightPacked);

                            var chunk = Storage.GetChunk(mipIndex);
                            chunk.Load();

                            fixed (byte* p = chunk.Data)
                            {
                                dataBoxArray[mipIndex].DataPointer = (IntPtr)p;
                                dataBoxArray[mipIndex].RowPitch = rowPitch;
                                dataBoxArray[mipIndex].SlicePitch = slicePitch;
                            }
                        }
                    }
                    _texture.InitializeFrom(_desc, new TextureViewDescription(), dataBoxArray);
                    _residentMips = _desc.MipLevels;
                }
            });

            return null;
        }
    }
}
