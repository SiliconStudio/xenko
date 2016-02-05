// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Data;

namespace SiliconStudio.Xenko.Assets.Textures
{
    [DataContract]
    [Display("Texture Settings")]
    public class TextureSettings : Configuration
    {
        public TextureSettings()
        {
            OfflineOnly = true;
        }

        /// <summary>
        /// Gets or sets the texture quality.
        /// </summary>
        /// <userdoc>The texture quality when encoding textures. Higher settings might result in much slower build depending on the target platform.</userdoc>
        [DataMember(0)]
        public TextureQuality TextureQuality = TextureQuality.Fast;
    }
}