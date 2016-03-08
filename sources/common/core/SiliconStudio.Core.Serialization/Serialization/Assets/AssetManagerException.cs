// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Serialization.Assets
{
    /// <summary>
    /// A subtype of <see cref="Exception"/> thrown by the <see cref="ContentManager"/>.
    /// </summary>
    class AssetManagerException : Exception
    {
        public AssetManagerException(string message) : base(message)
        {
        }

        public AssetManagerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}