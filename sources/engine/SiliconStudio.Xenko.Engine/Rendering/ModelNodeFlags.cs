// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
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
        /// Used by the physics engine to override the world matrix transform
        /// </summary>
        OverrideWorldMatrix = 4,

        /// <summary>
        /// The default flags.
        /// </summary>
        Default = EnableTransform | EnableRender       
    }
}
