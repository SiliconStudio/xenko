// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// The context used when building the thumbnail of an asset in a Package.
    /// </summary>
    public class ThumbnailCompilerContext : AssetCompilerContext
    {
        private readonly object thumbnailCounterLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbnailCompilerContext"/> class.
        /// </summary>
        public ThumbnailCompilerContext()
        {
            ThumbnailResolution = 128 * Int2.One;
        }

        /// <summary>
        /// Gets the desired resolution for thumbnails.
        /// </summary>
        public Int2 ThumbnailResolution { get; private set; }

        /// <summary>
        /// Indicate whether the fact that a thumbnail has been built should be notified with <see cref="NotifyThumbnailBuilt"/>
        /// </summary>
        /// <remarks>Use this property to avoid doing unnecessary stream operation when <see cref="ThumbnailBuilt"/> event has no subscriber.</remarks>
        public bool ShouldNotifyThumbnailBuilt { get { return ThumbnailBuilt != null; } }

        /// <summary>
        /// The array of data representing the thumbnail to display when a thumbnail build failed.
        /// </summary>
        public Task<Byte[]> BuildFailedThumbnail;

        /// <summary>
        /// The event raised when a thumbnail has finished to build.
        /// </summary>
        public event EventHandler<ThumbnailBuiltEventArgs> ThumbnailBuilt;

        /// <summary>
        /// Notify that the thumbnail has just been built. This method will raise the <see cref="ThumbnailBuilt"/> event.
        /// </summary>
        /// <param name="assetItem">The asset item whose thumbnail has been built.</param>
        /// <param name="result">A <see cref="ThumbnailBuildResult"/> value indicating whether the build was successful, failed or cancelled.</param>
        /// <param name="changed">A boolean indicating whether the thumbnal has changed since the last generation.</param>
        /// <param name="thumbnailStream">A stream to the thumbnail image.</param>
        /// <param name="thumbnailHash"></param>
        internal void NotifyThumbnailBuilt(AssetItem assetItem, ThumbnailBuildResult result, bool changed, Stream thumbnailStream, ObjectId thumbnailHash)
        {
            try
            {
                // TODO: this lock seems to be useless now, check if we can safely remove it
                Monitor.Enter(thumbnailCounterLock);
                var handler = ThumbnailBuilt;
                if (handler != null)
                {
                    // create the thumbnail build event arguments
                    var thumbnailBuiltArgs = new ThumbnailBuiltEventArgs
                    {
                        AssetId = assetItem.Id,
                        Url = assetItem.Location,
                        Result = result,
                        ThumbnailChanged = changed
                    };

                    Monitor.Exit(thumbnailCounterLock);
                    
                    // Open the image data stream if the build succeeded
                    if (thumbnailBuiltArgs.Result == ThumbnailBuildResult.Succeeded)
                    {
                        thumbnailBuiltArgs.ThumbnailStream = thumbnailStream;
                        thumbnailBuiltArgs.ThumbnailId = thumbnailHash;
                    }
                    else if (BuildFailedThumbnail != null)
                    {
                        thumbnailBuiltArgs.ThumbnailStream = new MemoryStream(BuildFailedThumbnail.Result);
                    }
                    handler(assetItem, thumbnailBuiltArgs);
                }
            }
            finally
            {
                if (Monitor.IsEntered(thumbnailCounterLock))
                    Monitor.Exit(thumbnailCounterLock);
            }
        }
    }
}