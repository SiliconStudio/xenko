// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Represents a way to instantiate a given <see cref="RenderObject"/> for rendering, esp. concerning effect selection and permutation.
    /// </summary>
    /// Note that a stage doesn't imply any order. You can render any combination of <see cref="RenderStage"/> and <see cref="RenderView"/> arbitrarily at any point of the Draw phase.
    public class RenderStage
    {
        /// <summary>
        /// The name of this <see cref="RenderStage"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The effect permutation slot name. If similar to another <see cref="RenderStage"/>, effect permutations will be shared.
        /// </summary>
        public string EffectSlotName { get; }

        /// <summary>
        /// Defines how <see cref="RenderNode"/> sorting should be performed.
        /// </summary>
        public SortMode SortMode { get; set; }

        /// <summary>
        /// Index in <see cref="RenderSystem.RenderStages"/>.
        /// </summary>
        public int Index = -1;

        public RenderStage(string name, string effectSlotName)
        {
            Name = name;
            EffectSlotName = effectSlotName;
        }

        /// <summary>
        /// Defines render targets this stage outputs to.
        /// </summary>
        [DataMemberIgnore]
        public RenderOutputDescription Output;

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}