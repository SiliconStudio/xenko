// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Core.Serialization.Contents
{
    /// <summary>
    /// A subtype of <see cref="Exception"/> thrown by the <see cref="ContentManager"/>.
    /// </summary>
    class ContentManagerException : Exception
    {
        public ContentManagerException(string message) : base(message)
        {
        }

        public ContentManagerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
