// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Packages
{
    /// <summary>
    /// Generic interface for logging. See <see cref="MessageLevel"/> for various level of logging.
    /// </summary>
    public interface IPackagesLogger
    {
        /// <summary>
        /// Logs the <paramref name="message"/> using the log <paramref name="level"/>.
        /// </summary>
        /// <param name="level">The level of the logged message.</param>
        /// <param name="message">The message to log.</param>
        void Log(MessageLevel level, string message);
    }
}
