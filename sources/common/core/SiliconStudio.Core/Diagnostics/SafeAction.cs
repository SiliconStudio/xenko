// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Diagnostics
{
    public static class SafeAction
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("SafeAction");

        [NotNull]
        public static ThreadStart Wrap(ThreadStart action, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            return () =>
            {
                try
                {
                    action();
                }
#if !SILICONSTUDIO_RUNTIME_CORECLR
                catch (ThreadAbortException)
                {
                    // Ignore this exception
                }
#endif
                catch (Exception e)
                {
                    Log.Fatal("Unexpected exception", e, CallerInfo.Get(sourceFilePath, memberName, sourceLineNumber));
                    throw;
                }
            };
        }

        [NotNull]
        public static ParameterizedThreadStart Wrap(ParameterizedThreadStart action, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            return obj =>
            {
                try
                {
                    action(obj);
                }
#if !SILICONSTUDIO_RUNTIME_CORECLR
                catch (ThreadAbortException)
                {
                    // Ignore this exception
                }
#endif
                catch (Exception e)
                {
                    Log.Fatal("Unexpected exception", e, CallerInfo.Get(sourceFilePath, memberName, sourceLineNumber));
                    throw;
                }
            };
        }
    }
}
