// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Instance of a model with its parameters.
    /// </summary>
    public interface IModelInstance
    {
        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        Model Model { get; }

        /// <summary>
        /// Gets the materials.
        /// </summary>
        /// <value>
        /// The materials.
        /// </value>
        List<Material> Materials { get; }
    }
}