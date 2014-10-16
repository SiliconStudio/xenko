using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiliconStudio.Paradox.Framework.MicroThreading;

namespace SiliconStudio.Paradox.DebugTools.DataStructures
{
    internal class MicroThreadPendingState
    {
        internal int ThreadId { get; set; }
        internal double Time { get; set; }
        internal MicroThreadState State { get; set; }
        internal MicroThread MicroThread { get; set; }
    }
}
