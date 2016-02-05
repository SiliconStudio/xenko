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
        [DataMember(0)]
        public TextureQuality TextureQuality = TextureQuality.Fast;
    }
}