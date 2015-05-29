// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// The different possible types of link between elements.
    /// </summary>
    [Flags]
    public enum ContentLinkType
    {
        /// <summary>
        /// A simple reference to the asset.
        /// </summary>
        Reference = 1,

        /// <summary>
        /// An inheritance between two assets.
        /// </summary>
        Inheritance = 2,

        /// <summary>
        /// An inheritance via composition between the two assets.
        /// </summary>
        CompositionInheritance = 4,

        /// <summary>
        /// All type of links.
        /// </summary>
        All = Reference | Inheritance | CompositionInheritance,
    }
}