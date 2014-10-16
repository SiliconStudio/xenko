// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Identify an object that is associated with an anchor file on the disk where all the <see cref="UPath"/> members of this 
    /// instance are relative to the <see cref="FullPath"/> of this instance.
    /// </summary>
    public interface IFileSynchronizable : IDirtyable
    {
        /// <summary>
        /// Gets the full path on disk where this instance is stored.
        /// </summary>
        /// <value>The full path.</value>
        UFile FullPath { get; }
    }
}