// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Diagnostics;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets.CompilerApp
{
    public class LogListenerRedirectToAction : LogListener
    {
        private readonly Action<string> logger;

        public LogListenerRedirectToAction(Action<string> logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            this.logger = logger;
        }

        /// <summary>
        /// Gets or sets the minimum log level handled by this listener.
        /// </summary>
        /// <value>The minimum log level.</value>
        public LogMessageType LogLevel { get; set; }

        protected override void OnLog(ILogMessage logMessage)
        {
            // Always log when debugger is attached
            if (logMessage.Type < LogLevel)
            {
                return;
            }

            logger(GetDefaultText(logMessage));
        }
    }
}