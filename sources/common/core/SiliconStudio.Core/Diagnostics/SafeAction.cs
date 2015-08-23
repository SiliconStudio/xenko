// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SiliconStudio.Core.Diagnostics
{
    public static class SafeAction
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("SafeAction");

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