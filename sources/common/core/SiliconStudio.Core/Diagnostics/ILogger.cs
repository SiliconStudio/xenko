// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// Interface for logging.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Gets the module this logger refers to.
        /// </summary>
        /// <value>The module.</value>
        string Module { get; }

        /// <summary>
        /// Logs the specified log message.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        void Log(ILogMessage logMessage);
    }
}
