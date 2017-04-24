// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Diagnostics;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// A <see cref="LoggerResult"/> that also forwards messages to another <see cref="ILogger"/>.
    /// </summary>
    [DebuggerDisplay("HasErrors: {HasErrors} Messages: [{Messages.Count}]")]
    public class ForwardingLoggerResult : LoggerResult
    {
        private readonly ILogger loggerToForward;

        public ForwardingLoggerResult(ILogger loggerToForward)
        {
            this.loggerToForward = loggerToForward;
        }

        protected override void LogRaw(ILogMessage logMessage)
        {
            base.LogRaw(logMessage);
            loggerToForward?.Log(logMessage);
        }
    }
}
