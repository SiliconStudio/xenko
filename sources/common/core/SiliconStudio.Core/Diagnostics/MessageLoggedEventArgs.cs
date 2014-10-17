// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// Arguments of the <see cref="Logger.MessageLogged"/> event.
    /// </summary>
    public class MessageLoggedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageLoggedEventArgs"/> class with a log message.
        /// </summary>
        /// <param name="message">The message that has been logged.</param>
        public MessageLoggedEventArgs(ILogMessage message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets the message that has been logged.
        /// </summary>
        public ILogMessage Message { get; private set; }
    }
}