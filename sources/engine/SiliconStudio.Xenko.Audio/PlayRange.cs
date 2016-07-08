using System;

namespace SiliconStudio.Xenko.Audio
{
    public struct PlayRange
    {
        public double Start;
        public double Length;

        public PlayRange(double start, double length)
        {
            Start = start;
            Length = length;
        }

        public double End
        {
            get { return Start + Length; }
            set { Length = value - Start; }
        }
    }
}
