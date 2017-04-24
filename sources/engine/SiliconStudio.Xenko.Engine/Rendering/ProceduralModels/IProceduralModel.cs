// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering.ProceduralModels
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

        void SetMaterial(string name, Material material);

        /// <summary>
        /// Gets the collection of material instances used by this <see cref="IProceduralModel"/>/
        /// </summary>
        [Display(Browsable = false)]
        IEnumerable<KeyValuePair<string, MaterialInstance>> MaterialInstances { get; }
    }
}
