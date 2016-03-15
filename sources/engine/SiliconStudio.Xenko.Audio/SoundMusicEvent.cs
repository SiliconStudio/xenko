// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// A <see cref="SoundMusic"/> event.
    /// </summary>
    internal enum SoundMusicEvent
    {
        ErrorOccurred,
        MetaDataLoaded,
        ReadyToBePlayed,
        EndOfTrackReached,
        MusicPlayerClosed,
    }
}
