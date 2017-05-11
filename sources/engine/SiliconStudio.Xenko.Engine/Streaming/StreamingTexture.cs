// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
        internal override bool CanBeUpdated => _streamingTask == null || _streamingTask.IsCompleted;

        internal void Init([NotNull] ContentStorage storage, ref ImageDescription imageDescription)
        {
            Init(storage);
            _desc = imageDescription;
            _residentMips = 0;

            if (_texture.GraphicsDevice != null)
                _texture.OnDestroyed();
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

        private async void StreamingTask(MicroThread microThread, int residency)
        {
            /*unsafe
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
            }*/

            for (int ressss = 1; ressss <= Description.MipLevels; ressss++)
            {
                if (IsDisposed)
                {
                    return;
                }

                var dataBoxArray = new DataBox[ressss];

                TextureDescription desc = _desc;
                desc.MipLevels = ressss;
                desc.Width = TotalWidth >> (_desc.MipLevels - ressss);
                desc.Height = TotalHeight >> (_desc.MipLevels - ressss);

                for (int mip = 0; mip < ressss; mip++)
                {
                    int mipIndex = _desc.MipLevels - ressss + mip;

                    int w = desc.Width >> mip;
                    int h = desc.Height >> mip;

                    int rowPitch, slicePitch;
                    int widthPacked;
                    int heightPacked;
                    Image.ComputePitch(Format, w, h, out rowPitch, out slicePitch, out widthPacked, out heightPacked);

                    var chunk = Storage.GetChunk(mipIndex);
                    Debug.Assert(chunk != null && chunk.Size == slicePitch);

                    //
                    var initialContext = SynchronizationContext.Current;
                    SynchronizationContext.SetSynchronizationContext(new MicrothreadProxySynchronizationContext(microThread));
                    var lockDatabase = Manager.ContentStreaming.MountDatabase();
                    await lockDatabase;
                    using (lockDatabase.Result)
                    {
                        chunk.Load();
                    }
                    SynchronizationContext.SetSynchronizationContext(initialContext);
                    //

                    unsafe
                    {
                        fixed (byte* p = chunk.Data)
                        {
                            dataBoxArray[mip].DataPointer = (IntPtr)p;
                            dataBoxArray[mip].RowPitch = rowPitch;
                            dataBoxArray[mip].SlicePitch = slicePitch;
                        }
                    }
                }

                _texture.OnDestroyed();

                _texture.InitializeFrom(desc, new TextureViewDescription(), dataBoxArray);
                _residentMips = ressss;

                await Task.Delay(1000);
            }
        }

        internal override Task CreateStreamingTask(int residency)
        {
            Debug.Assert(CanBeUpdated);

            var microThread = Scheduler.CurrentMicroThread;
            return _streamingTask = new Task(() => StreamingTask(microThread, residency));
        }
    }
}
