// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An AssetException.
    /// </summary>
    public class AssetException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetException"/> class.
        /// </summary>
        public AssetException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AssetException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="formattedArguments">The formatted arguments.</param>
        public AssetException(string message, params object[] formattedArguments)
            : base(message.ToFormat(formattedArguments))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public AssetException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
