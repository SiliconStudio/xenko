// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Engine;

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
        IndexingDictionary<Material> Materials { get; }
    }
}
