// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// The minimum parameters needed by a thumbnail build command.
    /// </summary>
    [DataContract]
    public class ThumbnailCommandParameters
    {
        public ThumbnailCommandParameters()
        {
        }

        public ThumbnailCommandParameters(string thumbnailUrl, Int2 thumbnailSize)
        {
            ThumbnailUrl = thumbnailUrl;
            ThumbnailSize = thumbnailSize;
        }

        public string ThumbnailUrl; // needed to force re-calculation of thumbnails when asset file is move

        public Int2 ThumbnailSize;

        public Vector3 UpAxis = Vector3.UnitY;

        public Vector3 FrontAxis = Vector3.UnitZ;
    }

    /// <summary>
    /// The parameters of a build command containing a typed reference to the asset to build.
    /// </summary>
    [DataContract]
    public class ThumbnailCommandParameters<T> : ThumbnailCommandParameters where T : Asset
    {
        public ThumbnailCommandParameters()
        {
        }

        public ThumbnailCommandParameters(T asset, string thumbnailUrl, Int2 thumbnailSize)
            : base(thumbnailUrl, thumbnailSize)
        {
            Asset = asset;
        }

        public T Asset;
    }
}