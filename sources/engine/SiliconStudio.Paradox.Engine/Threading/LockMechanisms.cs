// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SiliconStudio.Paradox.Threading
{
    public static class Locks
    {
        public static ILockMechanism Default;
        public static readonly ILockMechanism Standard = new StandardLock();
        public static readonly ILockMechanism Global = new GlobalLock();

        private static ILockMechanism _originalDefaultLockMechanism;

        static Locks()
        {
            SetDefaultLockMechanism(Standard);
            _originalDefaultLockMechanism = Default;
        }

        public static void SetDefaultLockMechanism(ILockMechanism defaultLockMechanism)
        {
            if (defaultLockMechanism == null)
                throw new ArgumentNullException("defaultLockMechanism");

            Default = defaultLockMechanism;
        }

        public static ILockMechanism RestoreDefaultLockMechanism()
        {
            ILockMechanism previous = Default;
            Default = _originalDefaultLockMechanism;
            return previous;
        }
    }

    public class StandardLock : ILockMechanism
    {
        public object OnBegin(object syncRoot, Action action)
        {
            Monitor.Enter(syncRoot);
            return syncRoot;
        }

        public void OnEnd(object syncRoot)
        {
            Monitor.Exit(syncRoot);
        }
    }

    public class GlobalLock : ILockMechanism
    {
        private static readonly object _globalLock = new object();

        public object OnBegin(object syncRoot, Action action)
        {
            Monitor.Enter(_globalLock);
            return null;
        }

        public void OnEnd(object syncRoot)
        {
            Monitor.Exit(_globalLock);
        }
    }
}
