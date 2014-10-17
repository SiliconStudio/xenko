// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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