// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// A SoundMusic action request aimed for the AudioEngine.
    /// </summary>
    internal struct SoundMusicActionRequest
    {
        public SoundMusic Requester;

        public SoundMusicAction RequestedAction;

        public SoundMusicActionRequest(SoundMusic requester, SoundMusicAction request)
        {
            Requester = requester;
            RequestedAction = request;
        }
    }
}
