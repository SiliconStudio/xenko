// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Core.Streaming
{
    /// <summary>
    /// The exception that is thrown when an internal error happened in the Audio System. That is an error that is not due to the user behavior.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public sealed class ContentStreamingException : Exception
    {
        /// <summary>
        /// Gets the storage container that causes this exception.
        /// </summary>
        public ContentStorage Storage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentStreamingException"/> class.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <param name="storage">The storage container.</param>
        public ContentStreamingException(string msg, ContentStorage storage = null)
            : base("An internal error happened in the content streaming service [details:'" + msg + "'")
        {
            Storage = storage;
        }
    }
}
