// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics.ProceduralModels
{
    /// <summary>
    /// Interface to create a procedural model.
    /// </summary>
    public interface IProceduralModel
    {
        /// <summary>
        /// Creates a procedural model.
        /// </summary>
        /// <param name="services">The services registry.</param>
        /// <param name="model">A model instance to fill with procedural content.</param>
        void Generate(IServiceRegistry services, Model model);
    }
}