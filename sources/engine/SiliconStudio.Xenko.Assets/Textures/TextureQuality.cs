// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Textures
{
    /// <summary>
    /// The desired texture quality.
    /// </summary>
    [DataContract("TextureQuality")]
    public enum TextureQuality // Matches PvrttWrapper.ECompressorQuality
    {
        Fast,
        Normal,
        High,
        Best,
    }
}
