// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// The desired texture quality.
    /// </summary>
    public enum TextureQuality // Matches PvrttWrapper.ECompressorQuality
    {
        Fast,
        Normal,
        High,
        Best,
    }
}
