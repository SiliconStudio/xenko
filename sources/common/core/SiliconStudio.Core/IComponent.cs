// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Core
{
    /// <summary>
    /// Base interface for all components.
    /// </summary>
    public interface IComponent : IReferencable
    {
        /// <summary>
        /// Gets the name of this component.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
    }
}

