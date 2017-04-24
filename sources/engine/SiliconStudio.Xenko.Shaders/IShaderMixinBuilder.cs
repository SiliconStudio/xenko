// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// Interface to be implemented for dynamic mixin generation.
    /// </summary>
    public interface IShaderMixinBuilder
    {
        /// <summary>
        /// Generates a mixin.
        /// </summary>
        /// <param name="mixinTree">The mixin tree.</param>
        /// <param name="context">The context.</param>
        void Generate(ShaderMixinSource mixinTree, ShaderMixinContext context);
    }
}
