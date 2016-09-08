// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Base interface for vector/color based compute color nodes.
    /// </summary>
    [InlineProperty]
    public interface IComputeColor : IComputeNode
    {
        /// <summary>
        /// Indicates if the IComputeColor has changed since the last time it was checked, which might require recompilation of the shader mixins.
        /// Once polled, it will reset all cached states and revert to false until other changes have been triggered.
        /// </summary>
        bool HasChanged { get; }        
    }
}