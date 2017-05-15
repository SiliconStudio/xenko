// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.MicroThreading;
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
        protected Task _streamingTask;

        internal StreamingTexture(StreamingManager manager, [NotNull] Texture texture)
            : base(manager)
        {
            _texture = texture;
            this.DisposeBy(_texture);
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
        /// Gets the number of textures in an array.
        /// </summary>
        public int ArraySize => _desc.ArraySize;

        /// <summary>
        /// Gets a value indicating whether this texture is a cube map.
        /// </summary>
        /// <value><c>true</c> if this texture is a cube map; otherwise, <c>false</c>.</value>
        public bool IsCubeMap => _desc.Dimension == TextureDimension.TextureCube;

        /// <summary>	
        /// Gets the texture texels format
        /// </summary>	
        public PixelFormat Format => _desc.Format;

        /// <summary>
        /// Gets index of the highest resident mip map (may be equal to MipLevels if no mip has been uploaded). Note: mip=0 is the highest (top quality)
        /// </summary>
        /// <returns>Mip index</returns>
        public int HighestResidentMipIndex => TotalMipLevels - _residentMips;

        /// <inheritdoc />
        public override object Resource => _texture;

        /// <inheritdoc />
        public override int CurrentResidency => _residentMips;

        /// <inheritdoc />
        public override int AllocatedResidency => Texture.MipLevels;

        /// <inheritdoc />
        internal override bool CanBeUpdated => _streamingTask == null || _streamingTask.IsCompleted;

        internal void Init([NotNull] ContentStorage storage, ref ImageDescription imageDescription)
        {
            Init(storage);
            _desc = imageDescription;
            _residentMips = 0;

            if (Texture.GraphicsDevice != null)
                Texture.OnDestroyed();
        }

        internal override Task UpdateAllocation(int residency)
        {
            return null;//todo: use UpdateSubresource and stream data to already allocated texture

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
                Texture.OnDestroyed();
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
                    Texture.InitializeFrom(desc);
                }
                else
                {
                    // TODO: create async task to resize texture and copy contents to the new one (GPU async task)
                    throw new NotImplementedException("StreamingTexture.UpdateAllocation: resize texture allocation");
                }
            }

            return result;
        }

        private async void StreamingTask(MicroThread microThread, int residency)
        {
            // Cache data
            var texture = Texture;
            int mipsChange = residency - CurrentResidency;
            int mipsCount = residency;
            Debug.Assert(mipsChange != 0);
            
            // TODO: allocation task should dispose texture or merge those tasks?
            if (residency == 0)
            {
                texture.OnDestroyed();
                return;
            }

            try
            {
                Storage.LockChunks();

                // Setup texture description
                TextureDescription newDesc = _desc;
                int newHighestResidentMipIndex = TotalMipLevels - mipsCount;
                newDesc.MipLevels = mipsCount;
                newDesc.Width = TotalWidth >> (_desc.MipLevels - newDesc.MipLevels);
                newDesc.Height = TotalHeight >> (_desc.MipLevels - newDesc.MipLevels);
                var dataBoxes = new DataBox[newDesc.MipLevels];

                // Get mips data
                for (int i = 0; i < newDesc.MipLevels; i++)
                {
                    int mipIndex = newHighestResidentMipIndex + i;
                    int mipWidth = TotalWidth >> mipIndex;
                    int mipheight = TotalHeight >> mipIndex;

                    int rowPitch, slicePitch;
                    int widthPacked;
                    int heightPacked;
                    Image.ComputePitch(Format, mipWidth, mipheight, out rowPitch, out slicePitch, out widthPacked, out heightPacked);

                    var chunk = Storage.GetChunk(mipIndex);
                    Debug.Assert(chunk != null && chunk.Size == slicePitch);
                    var data = await chunk.GetData(microThread);
                    if (!chunk.IsLoaded)
                        throw new DataException("Data chunk is not loaded.");

                    unsafe
                    {
                        fixed (byte* p = data)
                        {
                            dataBoxes[i].DataPointer = (IntPtr)p;
                            dataBoxes[i].RowPitch = rowPitch;
                            dataBoxes[i].SlicePitch = slicePitch;
                        }
                    }

                    if (IsDisposed)// TODO: use cancellation token
                        return;
                }

                // Recreate texture
                texture.OnDestroyed();
                texture.InitializeFrom(newDesc, new TextureViewDescription(), dataBoxes);
                _residentMips = newDesc.MipLevels;
            }
            finally
            {
                Storage.UnlockChunks();
            }
        }

        internal override Task CreateStreamingTask(int residency)
        {
            Debug.Assert(CanBeUpdated);

            var microThread = Scheduler.CurrentMicroThread;
            return _streamingTask = new Task(() => StreamingTask(microThread, residency));
        }

        /// <inheritdoc />
        protected override void Destroy()
        {
            // Stop streaming
            // TODO: stop steaming using cancellationToken

            base.Destroy();
        }
    }
}
