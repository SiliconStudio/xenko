// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Engine.Network
{
    [DataContract(Inherited = true)]
    public class SocketMessage
    {
        public int StreamId { get; set; }

        public static int NextStreamId
        {
            get { return Interlocked.Increment(ref globalStreamId); }
        }

        private static int globalStreamId;
    }
}
