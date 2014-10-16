// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Assets
{
    /// <summary>
    /// An interface that tags if an object is dirty.
    /// </summary>
    public interface IDirtyable
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is dirty.
        /// </summary>
        /// <value><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</value>
        bool IsDirty { get; set; }
    }
}