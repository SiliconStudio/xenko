using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiliconStudio.Xenko.Framework.MicroThreading;

namespace SiliconStudio.Xenko.DebugTools.DataStructures
{
    internal class MicroThreadPendingState
    {
        internal int ThreadId { get; set; }
        internal double Time { get; set; }
        internal MicroThreadState State { get; set; }
        internal MicroThread MicroThread { get; set; }
    }
}
