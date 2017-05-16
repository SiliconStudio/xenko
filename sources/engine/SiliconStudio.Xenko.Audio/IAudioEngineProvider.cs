// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Used internally to find the currently active audio engine 
    /// </summary>
    public interface IAudioEngineProvider
    {
        AudioEngine AudioEngine { get; }
    }
}
