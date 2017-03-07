// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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