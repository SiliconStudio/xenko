// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Represents a <see cref="RenderObject"/> drawn with a specific <see cref="RenderEffect"/>, with attached properties.
    /// </summary>
    public struct EffectObjectNode
    {
        /// <summary>
        /// The effect used.
        /// </summary>
        public RenderEffect RenderEffect;

        /// <summary>
        /// The object node reference.
        /// </summary>
        public ObjectNodeReference ObjectNode;

        /// <summary>
        /// The "PerObject" descriptor set.
        /// </summary>
        public DescriptorSet ObjectDescriptorSet;

        /// <summary>
        /// The "PerObject" constant buffer offset in our global cbuffer.
        /// </summary>
        public int ObjectConstantBufferOffset;

        public EffectObjectNode(RenderEffect renderEffect, ObjectNodeReference objectNode) : this()
        {
            RenderEffect = renderEffect;
            ObjectNode = objectNode;
        }
    }
}