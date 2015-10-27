using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiliconStudio.Paradox.Framework.MicroThreading;

namespace SiliconStudio.Paradox.DebugTools.DataStructures
{
    public class MicroThreadInfo
    {
        public long Id { get; set; }
        public MicroThreadState BeginState { get; set; }
        public MicroThreadState EndState { get; set; }
        public double BeginTime { get; set; }
        public double EndTime { get; set; }

        public MicroThreadInfo Duplicate()
        {
            MicroThreadInfo duplicate = new MicroThreadInfo();

            duplicate.Id = Id;
            duplicate.BeginState = BeginState;
            duplicate.EndState = EndState;
            duplicate.BeginTime = BeginTime;
            duplicate.EndTime = EndTime;

            return duplicate;
        }
    }
}
