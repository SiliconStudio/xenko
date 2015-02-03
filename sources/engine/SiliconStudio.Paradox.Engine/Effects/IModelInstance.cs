// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Effects
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
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        ParameterCollection Parameters { get; }

        /// <summary>
        /// Gets the draw order.
        /// </summary>
        /// <value>The draw order.</value>
        int DrawOrder { get; }
    }
}