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
        /// All type of links.
        /// </summary>
        All = Reference,
    }
}
