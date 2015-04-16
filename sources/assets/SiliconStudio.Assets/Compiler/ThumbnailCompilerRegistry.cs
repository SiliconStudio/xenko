// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// A registry containing the thumbnail compilers of the assets.
    /// </summary>
    public class ThumbnailCompilerRegistry : AttributeBasedRegistry<ThumbnailCompilerAttribute, IAssetCompiler>
    {
        private readonly Dictionary<Type, int> typeToCompilerPriority = new Dictionary<Type, int>();

        /// <summary>
        /// Gets the thumbnail compiler priority.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public int GetCompilerPriority(Type type)
        {
            int priority;
            typeToCompilerPriority.TryGetValue(type, out priority);
            return priority;
        }

        protected override bool ProcessAttribute(ThumbnailCompilerAttribute compilerAttribute, Type type)
        {
            if (!base.ProcessAttribute(compilerAttribute, type))
                return false;

            if (compilerAttribute.Priority != 0)
                typeToCompilerPriority[type] = compilerAttribute.Priority;

            return true;
        }
    }
}