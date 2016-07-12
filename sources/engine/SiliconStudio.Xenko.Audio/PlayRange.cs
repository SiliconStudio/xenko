using System;

namespace SiliconStudio.Xenko.Audio
{
    public struct PlayRange
    {
        public TimeSpan Start;
        public TimeSpan Length;

        public PlayRange(TimeSpan start, TimeSpan length)
        {
            Start = start;
            Length = length;
        }

        public TimeSpan End
        {
            get { return Start + Length; }
            set { Length = value - Start; }
        }
    }
}
