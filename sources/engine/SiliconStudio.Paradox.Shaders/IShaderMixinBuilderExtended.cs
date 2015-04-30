// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Shaders
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