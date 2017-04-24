// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Diagnostics;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// A <see cref="LogListener"/> implementation redirecting its output to a <see cref="Debug"/>.
    /// </summary>
    public class DebugLogListener : LogListener
    {
        protected override void OnLog([NotNull] ILogMessage logMessage)
        {
            Debug.WriteLine(GetDefaultText(logMessage));
            var exceptionMsg = GetExceptionText(logMessage);
            if (!string.IsNullOrEmpty(exceptionMsg))
            {
                Debug.WriteLine(exceptionMsg);
            }
        }
    }
}
