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
        /// <summary>
        /// An ID that will identify the message, in order to answer to it.
        /// </summary>
        public int StreamId;

        /// <summary>
        /// A state variable that can be used to report errors to the remote host.
        /// </summary>
        public int State = 0;

        public static int NextStreamId => Interlocked.Increment(ref globalStreamId);

        private static int globalStreamId;
    }
}
