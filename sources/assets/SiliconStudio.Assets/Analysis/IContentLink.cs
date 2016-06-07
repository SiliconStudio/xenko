// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// The interface for types representing a link between elements.
    /// </summary>
    public interface IContentLink
    {
        /// <summary>
        /// The reference to the element at the opposite side of the link.
        /// </summary>
        IReference Element { get; }

        /// <summary>
        /// The type of the link.
        /// </summary>
        ContentLinkType Type { get; }
    }
}
