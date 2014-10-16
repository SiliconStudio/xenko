// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Options used when searching asset dependencies.
    /// </summary>
    [Flags]
    public enum AssetDependencySearchOptions
    {
        /// <summary>
        /// Search for <c>in</c> only dependencies.
        /// </summary>
        In = 1,

        /// <summary>
        /// Search for <c>out</c> only dependencies.
        /// </summary>
        Out = 2,

        /// <summary>
        /// Search for <c>in</c> and <c>out</c> dependencies.
        /// </summary>
        InOut = In | Out,

        /// <summary>
        /// Search recursively
        /// </summary>
        Recursive = 4,

        /// <summary>
        /// Search recursively all <c>in</c> and <c>out</c> dependencies.
        /// </summary>
        All = InOut | Recursive
    }
}