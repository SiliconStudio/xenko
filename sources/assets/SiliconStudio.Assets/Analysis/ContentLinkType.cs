// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
