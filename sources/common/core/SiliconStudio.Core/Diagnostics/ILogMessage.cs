// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// The base interface for log messages used by the logging infrastructure.
    /// </summary>
    public interface ILogMessage
    {
        /// <summary>
        /// Gets or sets the module.
        /// </summary>
        /// <value>The module.</value>
        /// <remarks>
        /// The module is an identifier for a logical part of the system. It can be a class name, a namespace or a regular string not linked to a code hierarchy.
        /// </remarks>
        string Module { get; set; }

        /// <summary>
        /// Gets or sets the type of this message.
        /// </summary>
        /// <value>The type.</value>
        LogMessageType Type { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        string Text { get; set; }

        /// <summary>
        /// Gets or sets the exception info.
        /// </summary>
        ExceptionInfo ExceptionInfo { get; }
    }
}