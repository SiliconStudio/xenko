// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Threading;

namespace SiliconStudio.Core.MicroThreading
{
    public class MicrothreadProxySynchronizationContext : SynchronizationContext, IMicroThreadSynchronizationContext
    {
        private readonly MicroThread microThread;

        public MicrothreadProxySynchronizationContext(MicroThread microThread)
        {
            this.microThread = microThread;
        }

        MicroThread IMicroThreadSynchronizationContext.MicroThread => microThread;
    }
}
