// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// A notification of an SoundMusic event.
    /// </summary>
    internal struct SoundMusicEventNotification
    {
        public SoundMusicEvent Event;

        public object EventData;

        public SoundMusicEventNotification(SoundMusicEvent mEvent, object eventData)
        {
            Event = mEvent;
            EventData = eventData;
        }
    }
}