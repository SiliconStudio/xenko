// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Describe an play action that a SoundMusic can request to the AudioEngine
    /// </summary>
    internal enum SoundMusicAction
    {
        Play,
        Pause,
        Stop,
        Volume,
    }
}
