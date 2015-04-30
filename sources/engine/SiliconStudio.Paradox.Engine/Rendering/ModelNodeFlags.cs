// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Flags describing state of a <see cref="ModelNodeDefinition"/>.
    /// </summary>
    [Flags]
    [DataContract]
    public enum ModelNodeFlags
    {
        /// <summary>
        /// If true, <see cref="ModelNodeTransformation.Transform"/> will be used to update <see cref="ModelNodeTransformation.LocalMatrix"/>.
        /// </summary>
        EnableTransform = 1,

        /// <summary>
        /// If true, associated <see cref="Mesh"/> will be rendered.
        /// </summary>
        EnableRender = 2,

        /// <summary>
        /// The default flags.
        /// </summary>
        Default = EnableTransform | EnableRender,
    }
}