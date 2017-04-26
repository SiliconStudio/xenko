// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
