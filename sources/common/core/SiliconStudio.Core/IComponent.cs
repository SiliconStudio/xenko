// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Core
{
    /// <summary>
    /// Base interface for all components.
    /// </summary>
    public interface IComponent : IReferencable
    {
        /// <summary>
        /// Gets the id of this component.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the name of this component.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
    }
}

