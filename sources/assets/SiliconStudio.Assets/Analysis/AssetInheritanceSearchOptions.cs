// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Possible options used when searching asset inheritance.
    /// </summary>
    [Flags]
    public enum AssetInheritanceSearchOptions
    {
        /// <summary>
        /// Search for inheritances from base (direct object inheritance).
        /// </summary>
        Base = 1,

        /// <summary>
        /// Search for inheritances from compositions.
        /// </summary>
        Composition = 2,

        /// <summary>
        /// Search for all types of inheritances.
        /// </summary>
        All = Base | Composition,
    }
}