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

        public ThumbnailCommandParameters(Asset asset, string thumbnailUrl, Int2 thumbnailSize)
        {
            Asset = asset;
            ThumbnailUrl = thumbnailUrl;
            ThumbnailSize = thumbnailSize;
        }

        public Asset Asset;
        
        public string ThumbnailUrl; // needed to force re-calculation of thumbnails when asset file is move

        public Int2 ThumbnailSize;

        public Vector3 UpAxis = Vector3.UnitY;

        public Vector3 FrontAxis = Vector3.UnitZ;
    }
}