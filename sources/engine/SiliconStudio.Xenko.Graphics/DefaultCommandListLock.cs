// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Threading;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Used to prevent concurrent uses of CommandList against the main one.
    /// </summary>
    public struct DefaultCommandListLock : IDisposable
    {
        private readonly bool lockTaken;
        private object lockObject;

        public DefaultCommandListLock(CommandList lockObject)
        {
            if (lockObject.GraphicsDevice.InternalMainCommandList == lockObject)
            {
                this.lockObject = lockObject;
                lockTaken = false;
                Monitor.Enter(lockObject, ref lockTaken);
            }
            else
            {
                this.lockTaken = false;
                this.lockObject = null;
            }
        }

        public void Dispose()
        {
            if (lockTaken)
                Monitor.Exit(lockObject);
            lockObject = null;
        }
    }
}
