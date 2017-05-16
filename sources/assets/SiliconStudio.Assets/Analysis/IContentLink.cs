// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

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
