// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.ComputeEffect
{
    /// <summary>
    /// A compute effect
    /// </summary>
    public class ComputeEffect : DrawEffect
    {
        public ComputeEffect(DrawEffectContext context, string name)
            : base(context, name)
        {
        }

        /// <summary>
        /// Gets a render target with the specified description, scoped for the duration of the <see cref="DrawEffect.DrawCore"/>.
        /// </summary>
        /// <returns>A new instance of texture.</returns>
        protected Buffer NewScopedTypedBuffer(int count, PixelFormat viewFormat, bool isUnorderedAccess, GraphicsResourceUsage usage = GraphicsResourceUsage.Default)
        {
            return NewScopedBuffer(new BufferDescription(count * viewFormat.SizeInBytes(), BufferFlags.ShaderResource | (isUnorderedAccess ? BufferFlags.UnorderedAccess : BufferFlags.None), usage), viewFormat);
        }
    }
}