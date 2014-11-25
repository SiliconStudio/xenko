// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Shaders
{
    /// <summary>
    /// Extensions for <see cref="ShaderMixinSourceTree"/>
    /// </summary>
    public static class ShaderMixinSourceTreeExtensions
    {
        /// <summary>
        /// Adds the specified mixin source tree to the children of a <see cref="ShaderMixinSourceTree"/>.
        /// </summary>
        /// <param name="mixinSourceTreeChildren">The mixin source tree children.</param>
        /// <param name="sourceTree">The source tree.</param>
        public static void Add(this Dictionary<string, ShaderMixinSourceTree> mixinSourceTreeChildren, ShaderMixinSourceTree sourceTree)
        {
            // Overrides instead of adding so we can override children tree
            // TODO: This is not an optimized scenario, as it requires to instantiate a new ShaderMixinSourceTree
            mixinSourceTreeChildren[sourceTree.Name] = sourceTree;
        }
    }
}