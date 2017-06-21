// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Streaming;
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
        protected CancellationTokenSource _cancellationToken;
        protected MipInfo[] _mipsInfo;

        /// <summary>
        /// Helper structure used to pre-cache <see cref="StreamingTexture"/> mip maps metadata. Used to improve streaming performance (smaller CPU usage).
        /// </summary>
        protected struct MipInfo
        {
            public int Width;
            public int Height;
            public int RowPitch;
            public int SlicePitch;
            public int TotalSize; // SlicePitch * ArraySize

            public MipInfo(int width, int height, int rowPitch, int slicePitch, int arraySize)
            {
                Width = width;
                Height = height;
                RowPitch = rowPitch;
                SlicePitch = slicePitch;
                TotalSize = slicePitch * arraySize;
            }
        }

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
        public override int MaxResidency => _desc.MipLevels;

        /// <inheritdoc />
        internal override bool CanBeUpdated => _streamingTask == null || _streamingTask.IsCompleted;

        /// <inheritdoc />
        public override int CalculateTargetResidency(StreamingQuality quality)
        {
            if (MathUtil.IsZero(quality))
                return 0;

            var result = Math.Max(1, (int)(TotalMipLevels * quality));

            // Compressed formats have aligment restrictions on the dimensions of the texture (minimum size must be 4)
            if (Format.IsCompressed() && TotalMipLevels >= 3)
                result = MathUtil.Clamp(result, 3, TotalMipLevels);

            return result;
        }

        /// <inheritdoc />
        public override int CalculateRequestedResidency(int targetResidency)
        {
            int requestedResidency;
            
            // Check if need to increase it's residency or decrease
            if (targetResidency > CurrentResidency)
            {
                // Stream target quality in steps but lower mips at once
                requestedResidency = Math.Min(targetResidency, Math.Max(CurrentResidency + 4, 4));

                // Stream target quality in steps
                //requestedResidency = currentResidency + 1; 

                // Stream target quality at once
                //requestedResidency = targetResidency;
            }
            else
            {
                // Stream target quality in steps
                //requestedResidency = currentResidency - 1; 

                // Stream target quality at once
                requestedResidency = targetResidency;
            }

            return requestedResidency;
        }

        internal void Init([NotNull] ContentStorage storage, ref ImageDescription imageDescription)
        {
            if(imageDescription.Depth != 1)
                throw new ContentStreamingException("Texture streaming supports only 2D textures and 2D texture arrays.", storage);

            Init(storage);
            _desc = imageDescription;
            _residentMips = 0;
            CacheMipMaps();
        }

        private void CacheMipMaps()
        {
            var mipLevels = TotalMipLevels;
            _mipsInfo = new MipInfo[mipLevels];
            bool isBlockCompressed =
                (Format >= PixelFormat.BC1_Typeless && Format <= PixelFormat.BC5_SNorm) ||
                (Format >= PixelFormat.BC6H_Typeless && Format <= PixelFormat.BC7_UNorm_SRgb);

            for (int mipIndex = 0; mipIndex < mipLevels; mipIndex++)
            {
                int mipWidth, mipHeight;
                GetMipSize(isBlockCompressed, mipIndex, out mipWidth, out mipHeight);

                int rowPitch, slicePitch;
                int widthPacked;
                int heightPacked;
                Image.ComputePitch(Format, mipWidth, mipHeight, out rowPitch, out slicePitch, out widthPacked, out heightPacked);

                _mipsInfo[mipIndex] = new MipInfo(mipWidth, mipHeight, rowPitch, slicePitch, ArraySize);
            }
        }

        private void GetMipSize(bool isBlockCompressed, int mipIndex, out int width, out int height)
        {
            width = Math.Max(1, TotalWidth >> mipIndex);
            height = Math.Max(1, TotalHeight >> mipIndex);

            if (isBlockCompressed && ((width % 4) != 0 || (height % 4) != 0))
            {
                width = unchecked((int)(((uint)(width + 3)) & ~(uint)3));
                height = unchecked((int)(((uint)(height + 3)) & ~(uint)3));
            }
        }

        private void StreamingTask(int residency)
        {
            if (_cancellationToken.IsCancellationRequested)
                return;

            // Cache data
            var texture = Texture;
            int mipsChange = residency - CurrentResidency;
            int mipsCount = residency;
            Debug.Assert(mipsChange != 0);

            if (residency == 0)
            {
                // Release
                texture.ReleaseData();
                _residentMips = 0;
                return;
            }

            try
            {
                Storage.LockChunks();

                // Setup texture description
                TextureDescription newDesc = _desc;
                int newHighestResidentMipIndex = TotalMipLevels - mipsCount;
                newDesc.MipLevels = mipsCount;
                var topMip = _mipsInfo[_desc.MipLevels - newDesc.MipLevels];
                newDesc.Width = topMip.Width;
                newDesc.Height = topMip.Height;
                
                // Load chunks
                var mipsData = new IntPtr[mipsCount];
                for (int mipIndex = 0; mipIndex < mipsCount; mipIndex++)
                {
                    int totalMipIndex = newHighestResidentMipIndex + mipIndex;
                    var chunk = Storage.GetChunk(totalMipIndex);
                    if (chunk == null)
                        throw new ContentStreamingException("Data chunk is missing.", Storage);
                    
                    if (chunk.Size != _mipsInfo[totalMipIndex].TotalSize)
                        throw new ContentStreamingException("Data chunk has invalid size.", Storage);

                    var data = chunk.GetData(fileProvider);
                    if (!chunk.IsLoaded)
                        throw new ContentStreamingException("Data chunk is not loaded.", Storage);

                    if (_cancellationToken.IsCancellationRequested)
                        return;
                    
                    mipsData[mipIndex] = data;
                }

                // Get data boxes
                int dataBoxIndex = 0;
                var dataBoxes = new DataBox[newDesc.MipLevels * newDesc.ArraySize];
                for (int arrayIndex = 0; arrayIndex < newDesc.ArraySize; arrayIndex++)
                {
                    for (int mipIndex = 0; mipIndex < mipsCount; mipIndex++)
                    {
                        int totalMipIndex = newHighestResidentMipIndex + mipIndex;
                        var info = _mipsInfo[totalMipIndex];

                        dataBoxes[dataBoxIndex].DataPointer = mipsData[mipIndex] + info.SlicePitch * arrayIndex;
                        dataBoxes[dataBoxIndex].RowPitch = info.RowPitch;
                        dataBoxes[dataBoxIndex].SlicePitch = info.SlicePitch;
                        dataBoxIndex++;
                    }
                }

                if (_cancellationToken.IsCancellationRequested)
                    return;

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

        /// <inheritdoc />
        internal override Task StreamAsync(int residency)
        {
            Debug.Assert(CanBeUpdated && residency <= MaxResidency);

            _cancellationToken = new CancellationTokenSource();
            return _streamingTask = new Task(() => StreamingTask(residency), _cancellationToken.Token);
        }

        /// <inheritdoc />
        internal override void Release()
        {
            // Unlink from the texture
            this.RemoveDisposeBy(Texture);

            base.Release();
        }

        /// <inheritdoc />
        protected override void Destroy()
        {
            // Stop streaming
            if (_streamingTask != null && !_streamingTask.IsCompleted)
            {
                _cancellationToken.Cancel();
                _streamingTask.Wait();
            }
            _streamingTask = null;

            base.Destroy();
        }
    }
}
