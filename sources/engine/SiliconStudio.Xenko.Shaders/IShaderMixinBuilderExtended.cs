// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// Extension of <see cref="IShaderMixinBuilder"/> that provides keys and mixin informations.
    /// </summary>
    public interface IShaderMixinBuilderExtended : IShaderMixinBuilder
    {
        /// <summary>
        /// Gets an array of <see cref="ParameterKey"/> used by this mixin.
        /// </summary>
        /// <value>The keys.</value>
        ParameterKey[] Keys { get; }

        /// <summary>
        /// Gets the shaders/mixins used by this mixin.
        /// </summary>
        /// <value>The mixins.</value>
        string[] Mixins { get; }
    }
}
