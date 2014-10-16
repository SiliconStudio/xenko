// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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