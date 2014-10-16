// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SiliconStudio.Paradox.Threading
{
    public static class Lock
    {
        public static void Do(Action action)
        {
            Do(null, Locks.Global, action);
        }

        public static void Do(object syncRoot, Action action)
        {
            Do(syncRoot, Locks.Default, action);
        }

        public static void Do(object syncRoot, ILockMechanism lockMechanism, Action action)
        {
            if (lockMechanism == null)
                throw new ArgumentNullException("lockMechanism");

            object workingSyncRoot = null;

            try
            {
                workingSyncRoot = lockMechanism.OnBegin(syncRoot, action);
            }
            catch (Exception beginEx)
            {
                throw new LockMechanismException(LockMechanismStage.OnBegin, lockMechanism, beginEx);
            }

            try
            {
                action();
            }
            finally
            {
                try
                {
                    lockMechanism.OnEnd(workingSyncRoot);
                }
                catch (Exception endEx)
                {
                    throw new LockMechanismException(LockMechanismStage.OnEnd, lockMechanism, endEx);
                }
            }
        }
    }
}
